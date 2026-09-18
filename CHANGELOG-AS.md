# Change log (Asset Store)

Simplified changelog for the Asset Store listing's **Change log** field.
Paste the block below (plain text on purpose); newest version first.

---

1.0.22 - 2026-09-19

Added:
* Sample now ships MiniLM_uint8.sentis - a uint8-quantized MiniLM-L6-v2 model (~22 MB instead of ~90 MB), with the float32 source linked on Hugging Face and a ready-to-use script for regenerating quantized builds
* Defensive embedding normalization plus a load-time probe that warns when an assigned model does not emit unit-length vectors
* Bundled Apache-2.0 license text and third-party notices for the bundled model
* Extra editor tests locking in index storage invariants

Changed:
* Installation is via the .unitypackage from the store listing (git install references removed)
* The index database location is anchored to the project (Library/), not the process working directory
* The Tokens setting now drives tokenizer truncation and padding (changing it rebuilds the tokenizer)
* The search results view no longer depends on module registration order
* Backend property tooltip corrected (previously showed the Vocab description)
* Sensitivity documentation aligned with the shipped default of 25 (higher = stricter / fewer results)
* Large binaries in the repository are stored via Git LFS (no impact on the .unitypackage)
* Documentation now hosted at https://sniveler-code.github.io/docs_semantic-search.html

Fixed:
* Windows: the bundled sqlite3.dll is explicitly preloaded before first use (no PATH dependency); no change on macOS
* Crash when the Components category database was empty
* NaN progress bar when exactly one asset was checked
* Stale asset cache after a partial index clear
* Component embedding performance - names are embedded in a single batch per prefab and cached for the session

Removed:
* Float32 MiniLM.onnx (replaced by the quantized derivative, ~90 MB smaller package)
* Redundant vocab.txt (tokenizer.json already contains the full vocabulary)
* Unity.Plastic.Newtonsoft.Json dependency (standard Newtonsoft.Json is used instead)

1.0.0 - 2026-09-11 - First release of AI Semantic Search - local AI-powered asset search for the Unity Editor.

Added:
* Natural-language prefab search using Unity Sentis embeddings (MiniLM-L6-v2). 100% local: no API keys, no internet.
* Prefab metadata pipeline: names, components, folder/context, materials and mesh geometry; 253 built-in category keyword databases with automatic content-hash-based baking.
* SQLite project-local index (Library/SnivelerCode_SemanticIndex.db) - rebuildable at any time.
* SIMD-accelerated cosine similarity search with text-match bonus and sensitivity slider.
* Category coverage debug window (Window > SnivelerCode > Category Coverage (Debug)).
* Samples: MiniLM ONNX model, BERT tokenizer config and medieval demo prefabs.
* 30 EditMode unit tests; CI on Unity 2022.3 LTS and Unity 6.

Fixed:
* Window styles are now always applied from the package.
* Hyphenated and underscored keywords are split before embedding.
* Check/Index errors are now surfaced in the Status bar.

Known limitations:
* Index resets when Library/ is cleaned (re-run Check, then Index).
* Only BERT-like ONNX models (input_ids/attention_mask) are supported.
