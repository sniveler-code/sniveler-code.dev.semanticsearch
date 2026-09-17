# Third Party Notices

This package bundles or references the following third-party components.
Each component is the property of its respective copyright holder.

## 1. SQLite-net (bundled)

- **Files:** `Editor/ThirdParty/SQLite/SQLite.cs`, `Editor/ThirdParty/SQLite/SQLiteAsync.cs`, `Editor/ThirdParty/SQLite/AssemblyInfo.cs`
- **Author:** Krueger Systems, Inc.
- **License:** MIT — see the license header at the top of `SQLite.cs` (Copyright (c) 2009–2024 Krueger Systems, Inc.)
- **Source:** https://github.com/praeclarum/sqlite-net
- **Obligations:** the MIT copyright notice is preserved in the bundled files and listed here.

## 2. AI model — MiniLM / all-MiniLM-L6-v2 (bundled in Samples)

- **Files:** `Samples~/DemoContent/Medieval/Models/MiniLM_uint8.sentis` (uint8-quantized),
  `Samples~/DemoContent/Medieval/Data/tokenizer.json`
- **Author:** sentence-transformers / UKP Lab (Technische Universität Darmstadt); pretrained base model `nreimers/MiniLM-L6-H384-uncased` (MIT) by Niklas Reimers / Microsoft
- **Source:** https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2
- **Changes:** the shipped `MiniLM_uint8.sentis` is a uint8 weight-quantized derivative of a
  float32 ONNX conversion of the published model (Unity Sentis `ModelQuantizer`). Statement of
  changes: header of the bundled license file.
- **License:** **Apache-2.0** — the full, unmodified license text is bundled in this package at `Samples~/DemoContent/Medieval/Models/LICENSE-MiniLM.txt` (also available at https://www.apache.org/licenses/LICENSE-2.0)
- **Statement of changes (Apache-2.0 §4(b)):** the bundled `.onnx` is a float32 ONNX conversion of the model published in the source repository; the tokenizer configuration (`tokenizer.json`) is the original BERT WordPiece configuration from the same repository, unmodified (the complete vocabulary is embedded in its `model.vocab` section).
- **Redistribution note:** the model files remain licensed exclusively under Apache-2.0 and are **not** covered by this package's `LICENSE.md`. When redistributing the model, keep the bundled license file (`LICENSE-MiniLM.txt`) alongside the model files and preserve the notices in this document.

## 3. Unity Sentis (dependency, not bundled)

- **Package:** `com.unity.ai.inference` (Unity Sentis) — installed via Package Manager.
- **License:** Unity Companion License / Unity runtime — see https://unity.com/legal/licenses

---

*If you believe any component in this package is missing a required notice,
please open an issue at https://github.com/igor-karpushin/sniveler-code.dev.semanticsearch.*