# AI Semantic Search for Unity

**AI Semantic Search** is a high-performance Unity Editor extension that finds project assets by *meaning* instead of exact filenames.
It runs fully locally — embeddings are computed in the Editor with **Unity Sentis**, and the index is stored in **SQLite**. No API keys, no internet required.

## ✨ Features

- **Local AI**: everything runs on your machine (Unity Sentis + the bundled MiniLM model).
- **Prefab search**: understands names, components (Lights, VFX, Physics), folder/category context, materials and mesh geometry.
- **Vector search**: SIMD-accelerated cosine similarity for instant results, plus a text-match bonus.
- **Extensible**: add new asset kinds or custom metadata extractors via the `Metadata<T>` base class.

## 🚀 Quick Start

1. **Install** the package: in the Editor open **Assets → Import Package → Custom Package…**
   and select the downloaded **`AI Semantic Search.unitypackage`** (from the store listing).
   The `com.unity.ai.inference` (Unity Sentis) dependency installs automatically.
2. **Sample content** — the ~24 MB `MiniLM_uint8.sentis` model (uint8-quantized MiniLM-L6-v2),
   the tokenizer config and a small set of medieval prefabs ship **inside** the package file,
   so they are already in your project after the import. (If you imported a build without
   them: **Package Manager → AI Semantic Search → Samples → Base AI Model & Demo Assets →
   Import**.)
3. **Open the tool**: `Window > SnivelerCode > Semantic Search` (window title: *Asset AI Search*).
4. **Set up the model** in the **Embedding** tab:
   - **Model** ← `MiniLM_uint8.sentis` (from the sample);
   - **Vocab** ← `tokenizer.json` (same folder — this is the BERT tokenizer config!);
   - leave **Tokens = 128** and **Backend = GPU** defaults (switch to **CPU** if your GPU lacks compute shader support).
5. **Index assets**: in the **Prefabs tab → Check** (scans the project for new/modified prefabs) → **Index**.
6. **Search** in the Search tab — describe what you need, e.g. *"heavy axe"*, *"small green plant"* or *"loud explosion"*.
   Adjust the **Sensitivity** slider to loosen or tighten the results — the default 25 is loose
   (recall-first); raise it toward 60–70 for stricter, more precise matches.

## ⚙️ Configuration

| Setting | Default | Notes |
|---|---|---|
| `Model` (Embedding) | — | BERT-like Sentis model (`.sentis` or `.onnx`), e.g. the bundled `MiniLM_uint8.sentis` |
| `Vocab` (Embedding) | — | **tokenizer.json** (JSON tokenizer config with the vocabulary) |
| `Tokens` (Embedding) | 128 | max sequence length; raise to 256–512 for deep folder paths |
| `Backend` (Embedding) | GPU | Sentis compute backend; use CPU when GPU compute is unavailable |
| `Sensitivity` (Search) | 25 | minimum similarity score in %; higher = stricter / fewer results (raise to 60–70 for precision) |

The index and all settings live in `Library/SnivelerCode_SemanticIndex.db` (project-local, gitignored).

## 📂 Samples Contents

The imported sample contains:

- `MiniLM_uint8.sentis` — MiniLM-L6-v2 sentence-transformer, **uint8-quantized (~24 MB)**;
- `tokenizer.json` — BERT WordPiece tokenizer configuration (assign to the **Vocab** slot;
  the full vocabulary is embedded in this file);
- a small demo set of medieval prefabs to test the search right away.

## 🧠 Embedding model

The sample ships `MiniLM_uint8.sentis` — a **uint8 weight-quantized derivative** of
[sentence-transformers/all-MiniLM-L6-v2](https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2)
(Apache-2.0; see `Samples~/DemoContent/Medieval/Models/LICENSE-MiniLM.txt`). Quantization cuts the
file ~4× (90 MB → ~24 MB) with negligible effect on embedding quality for this model.

**Where to find the full-precision (float32) version:**

- The original float32 weights: <https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2>
  (Apache-2.0). The package ships only the lightweight quantized derivative; if you need the
  ~90 MB float32 build, download the model from the link above and follow the script below.

**Regenerating the quantized model** — place `MiniLM.onnx` anywhere in your project (e.g.
`Assets/MiniLM.onnx`) so the Sentis importer creates a `ModelAsset`, then run:

```csharp
using UnityEditor;
using UnityEngine;
using Unity.InferenceEngine;

public static class MiniLMQuantizer
{
    [MenuItem("Tools/AI Semantic Search/Quantize MiniLM (uint8)")]
    private static void Uint8() => Quantize(QuantizationType.Uint8, "MiniLM_uint8.sentis");

    [MenuItem("Tools/AI Semantic Search/Quantize MiniLM (float16)")]
    private static void Float16() => Quantize(QuantizationType.Float16, "MiniLM_float16.sentis");

    private static void Quantize(QuantizationType type, string outName)
    {
        var src = AssetDatabase.LoadAssetAtPath<ModelAsset>("Assets/MiniLM.onnx");
        if (src == null) { Debug.LogError("Place MiniLM.onnx in Assets/ first."); return; }

        Model model = ModelLoader.Load(src);
        ModelQuantizer.QuantizeWeights(type, ref model); // in memory, destructive

        string outPath = System.IO.Path.Combine(Application.dataPath, outName);
        ModelWriter.Save(outPath, model); // writes the .sentis file
        AssetDatabase.Refresh();
        Debug.Log($"Saved {outPath} ({type})");
    }
}
```

Note: `ModelLoader.Load(string)` reads the Sentis (`.sentis`) format only — ONNX files enter
through the importer, which is why the script loads via a `ModelAsset`. Copy the resulting
`.sentis` to where you keep models and assign it in the **Embedding** tab.

## 🧩 Extending

- `Metadata<T>` (`Editor/Transformers/Local/Metadata/Metadata.cs`) — base class for custom metadata extractors;
- register extractors in `LocalMetadataProcessor` (prefabs);
- transformers (`Transformer<T>`, e.g. `LocalTransformer`) orchestrate collection + vectorization;
- category keyword databases (`.json` under `Editor/Transformers/Local/Prefabs/Database/`) define the semantic axes used by the local context metadata.

> The **Gemini Transformer** is an experimental placeholder reserved for future PRO versions — it is listed in the Prefabs transformer dropdown but is not functional in this build.

## 📝 Requirements

- Unity **2022.3 LTS or newer** (developed and tested on Unity 6000.5.2f1);
- `com.unity.ai.inference` (Sentis) 2.3.0+;
- GPU compute shader support for the GPU backend (a CPU backend is available as fallback);
- Windows / macOS editor, no build-target requirements (Editor-only package).

## ⚠️ Known Limitations

- The index resets when `Library/` is cleaned — re-run Check → Index after that.
- Only BERT-like models (input_ids / attention_mask) are supported — `.onnx` (imported) and
  Sentis-serialized `.sentis` files.

## 📜 License

See `LICENSE.md` and `THIRD_PARTY_NOTICES.md` in the package root for software and AI-model licensing details.

---
Developed by **SnivelerCode** · [Documentation](https://sniveler-code.github.io/docs_semantic-search.html) · [GitHub](https://github.com/snivelercode)