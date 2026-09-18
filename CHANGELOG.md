# Changelog

All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **Quantized sample model** — the sample now ships `MiniLM_uint8.sentis`, a uint8
  weight-quantized build of MiniLM-L6-v2 (~22 MB instead of ~90 MB). The float32 source is
  no longer in the repository — the original float32 weights remain on Hugging Face. The
  README's new *Embedding model* section documents where to find them and the exact
  `ModelQuantizer` script for regenerating quantized builds.
- **Embedding normalization hardening** — `MiniProcessor` now normalizes every output vector
  (defensive; idempotent for the bundled MiniLM, whose ONNX graph already normalizes), and a new
  load-time probe (`ProbeOutputNormAsync`) warns in the status bar when a user-assigned model does
  not emit unit-length embeddings. `VectorMathTests` gained contract tests (normalization
  idempotency, dot-after-normalize == cosine similarity).
- **Model licensing compliance** — the full, unmodified Apache-2.0 license text is now
  bundled with the MiniLM model (`Samples~/DemoContent/Medieval/Models/LICENSE-MiniLM.txt`),
  `THIRD_PARTY_NOTICES.md` records the base-model origin (nreimers/MiniLM-L6-H384-uncased, MIT)
  and the statement of changes, and `LICENSE.md` explicitly carves the bundled model and
  tokenizer files out of the package license (they remain Apache-2.0).

### Changed
- **Order-independent service registration** — `PrefabsResultView` is now bound in
  `RegisterContainer()` instead of `PrefabsModule`'s constructor, so the search tab's
  `ISearchResult[]` no longer depends on the module resolve order (REVIEW M13).
- **Backend tooltip fixed** — the Embedding tab's **Backend** property showed the Vocab
  property's tooltip (copy-paste); it now describes the compute backend (REVIEW M12).
- **Sensitivity docs aligned with the implemented default** — the default is 25 (loose,
  recall-first) as the code has always shipped it; README/configure/getting-started/
  troubleshooting no longer claim a 70 default or a 60–70 "sweet spot" and now state the
  semantics (higher = stricter / fewer results). Two stale troubleshooting lines fixed in the
  same pass (`.sentis` models; removed Audios tab) (REVIEW M11).
- **Store-first installation** — README/docs now describe install via the `.unitypackage` from
  the store listing (Assets → Import Package → Custom Package); git-URL install references
  removed, `changelogUrl`/`documentationUrl` dropped from `package.json` (private repo), sample
  description updated to the quantized model.
- **`Tokens` setting now drives the tokenizer** — truncation and padding use the configured max
  length (128–512) instead of a hardcoded 128, and changing `Tokens` rebuilds the tokenizer
  (REVIEW M5). New `Encode_RespectsMaxLength` test pins the truncation/padding contract.
- **Git LFS for all large binaries** — the quantized model and the remaining large files (FBX
  models, textures, SQLite native binaries) are stored via Git LFS; added `.gitattributes` and
  `.gitignore` (the unpushed initial commit was amended, so the repository is LFS-native from
  the first push. The distributed `.unitypackage` contains real files, so LFS is irrelevant to
  end users).
- **Tokenizer truncator** — `MiniTokenizer` now uses `GenericTruncator` with `LongestFirstStrategy`
  instead of the obsolete `LongestFirstTruncator` (matches Sentis 2.6.1's `HuggingFaceParser`;
  behavior unchanged).

### Fixed
- **Component embedding performance** — `ComponentMetadata` now embeds custom component names in
  a single batch call per prefab and caches name → vector for the session (previously one
  inference per component per prefab, re-embedding the same scripts on every asset); tag output
  is unchanged (REVIEW M10).
- **Crash on empty Components database** — `ComponentMetadata.ProcessAsync` indexed
  `topTags[0]` unconditionally, throwing `IndexOutOfRangeException` when the Components
  category database had no rows; the component is now skipped (REVIEW M9).
- **NaN Check progress for a single asset** — `PrefabsModule.UpdateAssetsAsync` divided by
  `total - 1`, producing NaN in the progress bar when exactly one asset was scanned; now
  reports 100% in that case, 0→100 mapping unchanged otherwise (REVIEW M8).
- **Stale asset cache after partial index clear** — `SqliteStorage.ClearIndexes(AssetStorageType)`
  now invalidates the in-memory asset cache (it could otherwise keep serving deleted rows until
  the next write). Guard test added (REVIEW M7).

### Removed
- **Float32 `MiniLM.onnx`** — removed from the package and the repository to shrink the
  shipped package by ~90 MB; the shipped model is now the uint8-quantized derivative
  (see *Quantized sample model* above).
- **`vocab.txt` from the sample** — redundant: the code reads only `tokenizer.json`, whose
  `model.vocab` section already contains the complete WordPiece vocabulary (verified: no code
  path or asset reference used `vocab.txt`). Docs updated accordingly.
- **`Unity.Plastic.Newtonsoft.Json` dependency** — the tokenizer and tests now use standard
  `Newtonsoft.Json` (same pattern as the Sentis Tokenizer sample), the asmdefs reference
  `Newtonsoft.Json.dll` explicitly, and `com.unity.nuget.newtonsoft-json 3.2.1` is declared in
  `package.json`. The package no longer depends on the Plastic/collab-proxy VCS package being
  present in the editor (it was only resolved because Unity bundles Plastic by default).

## [1.0.0] - 2026-09-11 (pre-release)

### Added
- **Category coverage report** — debug window
  (`Window > SnivelerCode > Category Coverage (Debug)`) listing categories never matched
  by the indexed corpus and assets that match no category, to guide authoring of the
  keyword databases. Backed by the tested `CoverageReport` logic.
- **Semantic asset search** — natural-language search over prefabs using
  Unity Sentis embeddings (local, no API keys).
- **Prefab metadata pipeline** — name, components, folder/context, materials and geometry
  extractors feeding a semantic description; category keyword databases
  (General, Detailed, Components, Materials) with automatic baking.
- **SQLite storage** — `Library/SnivelerCode_SemanticIndex.db` (asset index, properties, metadata).
- **SIMD-accelerated search** — cosine similarity with a text-match bonus and sensitivity control.
- **Samples** — `Base AI Model & Demo Assets`: MiniLM ONNX model, BERT tokenizer config and
  medieval demo prefabs.
- **Editor tests** — 30 EditMode unit tests (vector math, tokenizer, storage, property roundtrip,
  category databases).
- **CI workflow** — GitHub Actions EditMode test run on Unity 2022.3 LTS and Unity 6 (6000.x).

### Changed
- Architecture refactor: `Editor/Embedding` → `Editor/Core/Embedding`, module facades
  (`IEmbeddingFacade`, `IPrefabsFacade`, `ISearchFacade`), deterministic UXML/USS loading
  from the package (TemplateProvider).
- Category databases are baked based on a content hash (FNV-1a): editing a JSON triggers
  an automatic re-bake on the next Check/Index, while unchanged databases are skipped.
- Context-matching thresholds (detailed 0.30, decay 0.15, general 0.25) are now editable
  settings in the Prefabs settings panel, persisted per project.
- Category re-bake is atomic (`MetadataReplace`): clearing and inserting happen in a single
  transaction, so a crash mid-bake can no longer leave a category empty.
- Category databases support an optional, project-local `examples` field (typical asset
  names of your project) that enriches the category embedding. Shipped databases stay
  generic on purpose — no demo-set fitting.
- Category content pass: new `robotics_automation` (General) and `input_control`
  (Components) categories; extended coverage for firearms, screens/holograms, modern
  vehicles, medical supplies and modern synthetics.
- Broad category coverage for genre-neutral game assets: 163 → **253 categories**
  (General 77, Detailed 138, Components 27, Materials 11) — VFX, weather, loot,
  interaction props, traps, markers, portals, destructibles, instruments, signage,
  roofs/fences/doors, vehicle parts, sci-fi props, ordnance, sports, water flow,
  funerary, art media and crafting stations.
- Content invariants are enforced by tests: unique ids across databases, Detailed
  parents resolve to General ids, no self-parent, no category pair shares ≥3 keys,
  and coverage floors.

### Fixed
- Window styles are now always applied from the package (previously depended on unassigned
  `[SerializeField]`).
- Hyphenated and underscored keywords (`close-quarters`, `heavy_ordnance`, …) are split
  before embedding instead of being baked as glued tokens.
- Category keywords joined with `_` (e.g. `heavy_ordnance`) are now split before
  embedding, so they no longer fall out of the tokenizer vocabulary.
- Removed debug console spam from the context-metadata similarity path.
- Errors during Check/Index are now surfaced in the Status bar.

### Removed
- Debug artifacts (`prefabs_*.txt`) and commented-out legacy code.
- Audio support (tab, transformers, metadata extractors, preview) — removed before release.

### Known limitations
- Index resets when `Library/` is cleaned (re-run Check → Index).
- Only BERT-like ONNX models (input_ids/attention_mask) are supported.
- The Gemini transformer is experimental and reserved for PRO versions.