using System.Collections.Generic;
using System.Data;
using Unity.InferenceEngine.Tokenization;
using Unity.InferenceEngine.Tokenization.Decoders;
using Unity.InferenceEngine.Tokenization.Mappers;
using Unity.InferenceEngine.Tokenization.Normalizers;
using Unity.InferenceEngine.Tokenization.Padding;
using Unity.InferenceEngine.Tokenization.PostProcessors;
using Unity.InferenceEngine.Tokenization.PostProcessors.Templating;
using Unity.InferenceEngine.Tokenization.PreTokenizers;
using Unity.InferenceEngine.Tokenization.Truncators;
using Unity.InferenceEngine.Tokenization.Truncators.Strategies;
using Newtonsoft.Json.Linq;

namespace SnivelerCode.SemanticSearch.Editor.Core.Embedding.MiniLM
{
    /// <summary>Builds a BERT WordPiece tokenizer from a tokenizer.json config.</summary>
    public static class MiniTokenizer
    {
        /// <summary>
        /// Creates the tokenizer from configuration text.
        /// <paramref name="maxLength"/> sets the truncation and padding length.
        /// </summary>
        public static Tokenizer Get(string text, int maxLength)
        {
            var config = JObject.Parse(text);

            var vocabulary = BuildVocabulary(config);
            var addedTokens = GetAddedTokens(config);

            vocabulary.TryGetValue("[CLS]", out int clsToken);
            vocabulary.TryGetValue("[SEP]", out int sepToken);
            vocabulary.TryGetValue("[PAD]", out int padToken);

            {
                var model = new WordPieceMapper(vocabulary, "[UNK]", "##", 100);
                var normalizer = new BertNormalizer(
                    cleanText: true, handleCjkChars: true, stripAccents: null, lowerCase: true);
                var preTokenizer = new BertPreTokenizer();
                var truncator = new GenericTruncator(new LongestFirstStrategy(), new RightDirectionRangeGenerator(), maxLength, 0);
                var postProcessor = new TemplatePostProcessor(
                    new Template(Template.Parse("[CLS]:0 $A:0 [SEP]:0")),
                    new Template(Template.Parse("[CLS]:0 $A:0 [SEP]:0 $B:1 [SEP]:1")),
                    new (string value, int id)[]
                    {
                        ("[CLS]", clsToken),
                        ("[SEP]", sepToken)
                    });
                var padding = new RightPadding(new FixedPaddingSizeProvider(maxLength),
                    new Token(padToken, "[PAD]"));

                var decoder = new WordPieceDecoder("##", true);

                return new Tokenizer(
                    model,
                    normalizer: normalizer,
                    preTokenizer: preTokenizer,
                    truncator: truncator,
                    postProcessor: postProcessor,
                    paddingProcessor: padding,
                    decoder: decoder,
                    addedVocabulary: addedTokens);
            }
        }

        /// <summary>Reads added and special tokens from the config.</summary>
        private static IEnumerable<TokenConfiguration> GetAddedTokens(JObject config)
        {
            var addedTokens = config["added_tokens"] as JArray;
            foreach (var addedToken in addedTokens!)
            {
                int id = addedToken["id"]!.Value<int>();
                string value = addedToken["content"]!.Value<string>();
                bool wholeWord = addedToken["single_word"]!.Value<bool>();
                var strip = (addedToken["lstrip"]!.Value<bool>() ? Direction.Left : Direction.None) |
                            (addedToken["rstrip"]!.Value<bool>() ? Direction.Right : Direction.None);
                bool normalized = addedToken["normalized"]!.Value<bool>();
                bool special = addedToken["special"]!.Value<bool>();

                yield return new TokenConfiguration(id, value, wholeWord, strip, normalized, special);
            }
        }

        /// <summary>Builds the token-id vocabulary from the config.</summary>
        private static Dictionary<string, int> BuildVocabulary(JObject config)
        {
            var output = new Dictionary<string, int>();
            var vocab = config["model"]["vocab"] as JObject;

            foreach ((string value, JToken id) in vocab!)
                output[value] = id?.Value<int>() ?? throw new DataException($"No id for value {value}");

            if (config["added_tokens"] is JArray addedTokens)
            {
                foreach (var addedToken in addedTokens)
                {
                    string content = addedToken["content"]!.Value<string>();
                    if (output.ContainsKey(content))
                    {
                        continue;
                    }

                    int id = addedToken["id"]!.Value<int>();
                    output.Add(content, id);
                }
            }

            return output;
        }
    }
}
