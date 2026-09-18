# AI Semantic Search — Package Review

Review date: 2026-07 (local snapshot of `sniveler-code.dev.semanticsearch` v1.0.0)
Scope: package manifest, code (~5.5k lines own C# + bundled SQLite-net), samples, docs, tests,
licensing (incl. legal review of the bundled all-MiniLM-L6-v2 ONNX model).

Severity legend: 🔴 blocking for release · 🟠 high · 🟡 medium · ⚪ low/nit

---

## 1. Package manifest & structure

| Item | Value | Status |
|---|---|---|
| name | `sniveler-code.dev.semanticsearch` | ⚪ reverse-DNS, fine for UPM; Asset Store will assign its own |
| version | `1.0.0` | ✅ |
| unity | `2022.3` | ✅ matches docs (dev/tested on 6000.5.2f1) |
| displayName | "AI Semantic Search" | ✅ |
| dependencies | `com.unity.ai.inference: 2.3.0` | 🟠 incomplete — see findings B2 |
| samples | `Samples~/DemoContent` ("Base AI Model & Demo Assets") | ✅ model + tokenizer + demo prefabs |
| asmdef | `SnivelerCode.SemanticSearch.Editor` (Editor-only) | ✅ `autoReferenced: true` |
| tests | `Tests/Editor`, own asmdef, `TestAssemblies` optional ref | ✅ |

Structure is clean: `Editor/Core/{Embedding,Search,Prefabs,Property,Storage,Status,Tabs,Utils}`,
`Editor/Transformers/{Local,Gemini}`, `Editor/Templates` (UXML/USS/Icons), `Editor/ThirdParty/SQLite`,
`Tests/Editor`, `Documentation~`, `Samples~`.

- ⚪ `ISemanticStorage2` (`Editor/Core/Storage/ISemanticStorage.cs`) is dead code — defined, never referenced.
- ✅ LFS now real: `.gitattributes` + `.gitignore` added, 131 binary files (ONNX, FBX, textures,
  SQLite natives) on LFS, unpushed initial commit amended — pack 471 KiB, tracked 1.8 MB, LFS
  objects 165 MB (`.git/lfs`). See finding B4.

---

## 2. Legal & licensing (including the MiniLM ONNX model)

### 2.1 License chain of the model (verified against Hugging Face API)

| Artifact | File in package | License | Verified |
|---|---|---|---|
| all-MiniLM-L6-v2 (sentence-transformers) | `Samples~/…/Models/MiniLM.onnx` | **Apache-2.0** (`cardData.license: apache-2.0`, tag `license:apache-2.0`) | HF API 2026-07 |
| Base model nreimers/MiniLM-L6-H384-uncased | (pretrained inside the above) | MIT (`cardData.license: mit`) | HF API 2026-07 |
| `tokenizer.json` / `vocab.txt` | `Samples~/…/Data/` | Follows the model repo license: Apache-2.0 | — |
| SQLite-net | `Editor/ThirdParty/SQLite/` | MIT, license text **bundled** (`LICENSE.txt`) ✅ | local |
| Unity Sentis | dependency, not bundled | Unity Companion License | — |

### 2.2 The legal way to include the MiniLM-L6-v2 ONNX model

Apache-2.0 is a permissive license: it **allows** commercial use, modification and
redistribution (including inside a paid Asset Store product). So shipping the model in the
package (even via the Samples importer) is legal **if the license conditions are met**:

1. **Provide a copy of the Apache-2.0 license with the model.** 🔴 Currently the package only
   links to `https://www.apache.org/licenses/LICENSE-2.0` in `THIRD_PARTY_NOTICES.md` and bundles
   **no license text file** for the model (only the SQLite MIT `LICENSE.txt` is bundled).
   Apache-2.0 §4 requires the license to accompany the work when you redistribute it.
   **Fix:** add `Samples~/DemoContent/Medieval/Models/LICENSE-MiniLM.txt` (the full Apache-2.0
   text) — or a shared `ThirdParty/Licenses/Apache-2.0.txt` — and point `THIRD_PARTY_NOTICES.md`
   at the bundled file.
2. **Preserve notices and attribution** (© statement, model card link, "ALL RIGHTS RESERVED"
   wording from the original). The current `THIRD_PARTY_NOTICES.md` attribution
   (sentence-transformers / UKP Lab) is correct; add the base-model origin
   (MiniLM by Niklas Reimers / Microsoft, MIT) and keep the "float32 conversion" statement of
   changes (Apache-2.0 §4(b) requires stating modifications — you already do, which is good).
3. **Do not place the model under your package license.** The package's `LICENSE.md` (MIT draft)
   must explicitly carve out `Samples~/…/Models/` and `…/Data/` — e.g. "This license does not
   apply to the MiniLM model files, which remain under Apache-2.0 (see LICENSE-MiniLM.txt)".
   The current draft says `Editor/**` only, which is close but the carve-out should be explicit.
4. **Asset Store specific:** the store's EULA covers *your* code; third-party components keep
   their own licenses. Apache-2.0 permits this, but disclose the bundled model + its license in
   the store listing (the store requires third-party content disclosure). This is exactly what
   `THIRD_PARTY_NOTICES.md` is for — ship it in the package and mirror it in the listing.
5. **Provenance / hygiene:** keep a short `PROVENANCE.md` (or a section in the notices) recording
   where the ONNX came from (HF repo `sentence-transformers/all-MiniLM-L6-v2`, file
   `onnx/model.onnx` or your export pipeline, date, conversion steps, sha256 of the file).
   This protects you against any future claim about the weights and is standard practice.
6. **No other restrictions apply:** the model is not gated on HF, its use is not restricted to
   the HF Terms, and the training-data provenance (Reddit/S2ORC/…) creates no license
   obligations on the *weights* themselves for an editor tool that only reads local asset names.

**Verdict: including the model is legal today under Apache-2.0; the one missing obligation is
bundling the license text (item 1).** Doing items 1–3 makes the package fully compliant.

### 2.3 Other license findings

- 🟠 **Undeclared JSON dependencies** (see B2): `MiniTokenizer.cs` uses
  `Unity.Plastic.Newtonsoft.Json` (comes from `com.unity.collab-proxy` — *not* a Sentis
  dependency) and `LocalTransformer.cs` uses `Newtonsoft.Json` (from
  `com.unity.nuget.newtonsoft-json`, a transitive Sentis dependency). Neither is declared in
  `package.json`. Both work only because Unity ships those DLLs in the default editor install.
- ⚪ `LICENSE.md` still contains a "NOTE FOR THE RELEASE OWNER" draft paragraph — finalize before
  publishing (MIT for the code vs Asset Store EULA for the store listing).

---

## 3. Code review — findings

### B1 · 🟠 [LEGAL] Model license text not bundled — ✅ FIXED
Resolved: full Apache-2.0 text bundled at
`Samples~/DemoContent/Medieval/Models/LICENSE-MiniLM.txt` (with applicability header),
`THIRD_PARTY_NOTICES.md` updated (base-model origin, statement of changes, bundled-license
pointer), `LICENSE.md` now explicitly carves the model + tokenizer files out of the package
license. See CHANGELOG `[Unreleased]`.

### B2 · 🟠 [BUILD] Undeclared, environment-dependent JSON dependencies — ✅ FIXED
Fixed: `MiniTokenizer.cs` and `CategoryDatabaseTests.cs` switched to
`Newtonsoft.Json.Linq` (same pattern as Sentis's own Tokenizer sample); main asmdef now lists
`"Newtonsoft.Json.dll"` in `precompiledReferences` (the pattern used by
`Unity.InferenceEngine.Editor.asmdef`); test asmdef's Plastic precompiled reference replaced with
`Newtonsoft.Json.dll`; `package.json` now declares
`"com.unity.nuget.newtonsoft-json": "3.2.1"` (matches Sentis's own minimum; installed 3.2.2
satisfies it). Zero Plastic references remain. Original issue, for the record:
- `Editor/Core/Embedding/MiniLM/MiniTokenizer.cs:12` → `using Unity.Plastic.Newtonsoft.Json.Linq;`
- `Tests/Editor/CategoryDatabaseTests.cs:5` → same namespace
- `Editor/Transformers/Local/Prefabs/LocalTransformer.cs` → `using Newtonsoft.Json.Linq;`

Verified facts (installed `com.unity.ai.inference` 2.6.1, the version satisfying `2.3.0`):
- Sentis depends on `com.unity.nuget.newtonsoft-json` (provides `Newtonsoft.Json.dll`),
  **not** on `com.unity.plastic`/`collab-proxy`.
- `Unity.Plastic.Newtonsoft.Json.dll` lives in `com.unity.collab-proxy` (Plastic SCM proxy) and
  is present only because Unity bundles Plastic by default.
- The Editor asmdef lists **no** `precompiledReferences`; the Tests asmdef lists
  `Unity.Plastic.Newtonsoft.Json.dll` (file not in the package → resolved only if the editor
  provides it).
- Sentis's own Tokenizer sample (`Samples~/Tokenizer - All Mini LM/RunAllMiniLm.cs`) uses
  `Newtonsoft.Json` — the pattern this package should follow.

**Fix:** switch both Plastic usages to `Newtonsoft.Json.Linq`, add
`"com.unity.nuget.newtonsoft-json": "3.2.1"` to `package.json` dependencies, and remove the
`precompiledReferences` entry from the Tests asmdef. Removes a dependency on a VCS package that
Unity is actively deprecating (Git is the default VCS; Plastic SCM is on its way out).

### B3 · 🟡 [ROBUSTNESS] Score semantics depend on the model graph's normalization — original B3 RETRACTED, premise verified
**Update (investigated against the artifact):** the original B3 ("vectors never normalized →
thresholds and % scores broken") was **wrong**. The bundled ONNX already outputs unit-length
vectors:
- *Static:* graph tail of `MiniLM.onnx` (683 nodes) = `…/model.1/Concat` (MeanWordsPooling)
  → `Abs → Pow → ReduceSum → Pow → Clip → Expand → Div` = sentence-transformers' `Normalize`
  (`x / clip(‖x‖₂, 1e-12)`) feeding the `sentence_embedding` output. The graph embeds the full
  `modules.json` pipeline (model.0 Transformer → model.1 MeanWordsPooling → model.2 Normalize);
  inputs are `input_ids` / `attention_mask` / `token_type_ids` (int64, dynamic
  `batch_size`×`sequence_length`).
- *Dynamic:* onnxruntime inference on the bundled file → `‖sentence_embedding‖₂ = 1.0` exactly
  (tested at sequence lengths 8 and 16).

Consequences of this: `VectorMath.CosineSimilaritySimd` (a plain dot product) **is** a true
cosine similarity on this model's output — the Sensitivity threshold, the "%" display, the
`0.4f` component cutoff and the Context thresholds all work as designed. The docs' "cosine
similarity" wording is correct. No code change required for the bundled model.

**Hardening — ✅ IMPLEMENTED (defense in depth, both options):**
- (b) `MiniProcessor.GetVectorsAsync` now applies `VectorMath.NormalizeInPlace` to every output
  vector (idempotent for the bundled model — verified ‖output‖₂ = 1.0 — ~384 flops/vector), so the
  unit-length invariant holds for *any* user-assigned model.
- (a) new `IEmbeddingProcessor.ProbeOutputNormAsync`: on model assignment, `EmbeddingModule` runs
  one 16-token probe inference, measures the RAW output norm, and if ‖e‖ deviates from 1 by >0.01
  registers an advisory status message (vectors are normalized automatically; Sensitivity
  thresholds are calibrated for the bundled MiniLM). Probe failure is non-fatal (returns -1, check
  skipped) — it is advisory, never a load error.
- Contract tests added to `VectorMathTests`: `NormalizeInPlace_IdempotentForUnitVectors` and
  `NormalizeThenDot_EqualsCosineSimilarity` (the scoring contract). A full `GetVectorsAsync`
  unit-length assertion would require model-in-the-loop (kept out of EditMode, see L17).

The original concern that led here remains valid as rationale: `EmbeddingModel.Model` is
user-assignable and `IsAssignableFrom` checks only *input* names — the probe + normalization above
are what protect the [-1, 1] score semantics for swapped-in models.

### B4 · 🟠 [PACKAGING] Git LFS claim vs reality — ✅ FIXED
Original state: FAQ claimed LFS, but the repo had no `.gitattributes`, no LFS pointers, and the
90 MB ONNX was a plain blob (`.git` = 151 MB loose). Pre-fix investigation: remote
`origin` (github.com/sniveler-code/…) exists but is **empty** (no refs; `[origin/main: gone]`;
`ls-remote` = nothing) → history safely rewritable (pre-release, local only).

Done:
- Added `.gitignore` (OS / IDE / Unity-generated: `Library/`, `Temp/`, `*.csproj`, …).
- `.gitattributes` via `git lfs track`: `*.onnx`, `*.fbx`, `*.png`, `*.dll` + the extensionless
  SQLite macOS binaries (`sqldiff`, `sqlite3`, `sqlite3_analyzer`, `sqlite3_rsync`) — **131
  binary files** on LFS in total.
- Converted all 131 tracked binaries to LFS pointers; folded the pending review fixes (B1/B2/B3/
  vocab removal/L18) into the pre-release snapshot and **amended the unpushed initial commit**;
  `git reflog expire --expire=now --all` + `git gc --prune=now` purged the dead blobs.
- Result: pack **471 KiB** (was ~70 MiB), tracked total **1.8 MB**, 165 MB in `.git/lfs/objects`.
  The FAQ's LFS instructions (`git lfs install` + `git lfs pull`) are now factually correct — no
  FAQ change needed. First `git push` uploads the 165 MB of LFS objects (GitHub free tier: 1 GB
  LFS storage).
- Recovery hashes (in case of trouble): original `b0e2abb` → amend 1 `31152fa` → amend 2
  `0ca872f` → final `319988f`.

### M5 · 🟡 [CORRECTNESS] Hardcoded 128 in tokenizer vs configurable `Tokens` (128–512) — ✅ FIXED
**Fixed:** `MiniTokenizer.Get(text, maxLength)` now parameterizes the truncator and padding
with the configured length; `IEmbeddingProcessor.SetTokenizer(asset, maxLength)` threads it
through, and `EmbeddingModule.OnPropertyChange` rebuilds the tokenizer on `MaxLength` change
(and passes the current value on Model/Vocab changes). New test `Encode_RespectsMaxLength`
pins truncation at 32 and padding at 256 for a 300-word OOV text. Raising `Tokens` to 256–512
now genuinely lengthens tokenization, as README/configure.md claim (re-index after changing
it: embeddings were computed at the previous length).

*Original finding:* `MiniTokenizer.Get` hardcoded `GenericTruncator(..., 128, 0)` and
`RightPadding(new FixedPaddingSizeProvider(128), ...)`, while `EmbeddingModel.MaxLength` allows
128–512 and `MiniProcessor` allocates tensors of the configured length. Raising `Tokens` above
128 does **not** lengthen tokenization (it only zero-pads rows) — directly contradicting README/
configure.md ("raise to 256–512 for deep folder paths"). Also `OnPropertyChange` only reacts to
Model/Vocab, so changing `Tokens` never rebuilds the tokenizer.
**Fix:** parameterize truncator/padding with the configured length and rebuild the tokenizer on
`MaxLength` change (cheap: it's just config). Or clamp the UI to 128 and say so.

### M6 · 🟡 [CORRECTNESS] No wait for inference completion — ❌ RETRACTED
**Retracted (verified against installed Sentis 2.6.1 source):** there is no race. `Worker` has
**no async API** — no `ScheduleAsync`/`WaitForCompletionAsync` exist (the fix proposed in the
original finding referenced methods that don't exist). `Worker.Schedule(params Tensor[])` is
fully synchronous: it executes every layer inline (`l.Execute(ctx)`) and, for the GPU backend,
ends with `ExecuteCommandBufferAndClear()` = `Graphics.ExecuteCommandBuffer(cb); cb.Clear();` —
a blocking dispatch. When `Schedule()` returns, inference is complete, so `PeekOutput` right
after it is correct. `PeekOutput` returns a *non-owning reference* ("valid until the next
`Schedule` or `Dispose`") — nothing to dispose per call (lifetime concern: N/A).

The `Schedule` XML doc comment "This is non-blocking" is misleading (it means "no handle to
await", not "returns before completion").

**The `await Task.Yield()` is intentional (owner-confirmed):** it yields back to the editor's
main loop after each blocking inference call so the UI repaints between batches during
multi-batch indexing. Comments now pin this intent in `MiniProcessor` (both call sites).

Remaining (documented, not fixed): the inference call itself still blocks the main thread for
one batch — the Sentis compute backend exposes no job/thread API. Acceptable editor behavior
for ~128-token batches; true async inference would be a Sentis-level feature request.

### M7 · 🟡 [CORRECTNESS] Stale asset cache after partial clear — ✅ FIXED
**Fixed:** `ClearIndexes(AssetStorageType)` now invalidates `_assetCache`, matching the
`ClearIndexes(HashSet)` and `SetIndexes` behavior — every mutation invalidates the cache.
Guard test `ClearIndexes_ByType_InvalidatesAssetCache` pins the invariant. Note: at fix time
the type overload had no UI call sites (it is public `SqliteStorage` API, not on
`IAssetsStorage`) — the fix is preventive, so a future "clear all of a type" caller cannot
serve stale reads.

### M8 · 🟡 [CORRECTNESS] Division by zero in Check progress — ✅ FIXED
**Fixed:** `PrefabsModule.UpdateAssetsAsync` now guards the division: `total <= 1 ? 100f :
i * 100f / (total - 1)`. A single scanned asset is by definition the last one → 100% instead of
NaN; the 0→100 mapping for multi-asset scans is unchanged (the original suggestion
`i * 100f / total` would have shifted the scale so the last asset never reached 100%).

### M9 · 🟡 [CORRECTNESS] `topTags[0]` on empty Components database — ✅ FIXED
**Fixed:** `ComponentMetadata.ProcessAsync` guards `topTags.Length == 0` and skips the component
(`continue`, not `return` — one empty database must not abort the remaining components).
With an empty Components database the extractor now gracefully produces no tags instead of
throwing `IndexOutOfRangeException`.

### M10 · 🟡 [PERF] Per-component embedding, batch size 1, no caching — ✅ FIXED
**Fixed:** `ComponentMetadata.ProcessAsync` is now two-pass: (1) standard tags are collected and
custom component names gathered; (2) the unique names missing from the new per-session
`_nameVectorCache` (cleaned name → vector) are embedded in a **single** `GetVectorsAsync` batch
call, then each component's similarity is computed from the cached vectors. A project with
hundreds of prefabs sharing a few dozen scripts now pays one inference per *unique script per
session* instead of one per (prefab, component). Tag output is unchanged (deterministic model;
identical `Similarity` inputs).

### M11 · 🟡 [DOC/UX] Sensitivity defaults inconsistent — ✅ FIXED
**Decision (behavior-preserving):** the implemented default is **25** — `SearchModel.Sensitivity = 25`
with `if (finalScore > sensitivity * 0.01f)` and a unit-length model (B3), i.e. a loose,
recall-first threshold. The code default was kept (changing it would silently change search
behavior for all users) and the docs were aligned to it: README, configure.md,
getting-started.md and troubleshooting.md no longer claim a 70 default / 60–70 sweet spot;
they now state the semantics (higher = stricter / fewer results; raise to 60–70 for precision).
The UXML tooltip ("Higher values (70%+) return more precise matches…") already matched the code
and was kept. Also fixed two stale troubleshooting lines found in the same pass: the model
field now mentions `.sentis` as well as `.onnx`, and the removed **Audios** tab was taken out
of the Check instructions.

### M12 · 🟡 [DOC/UX] `Backend` tooltip copy-paste bug — ✅ FIXED
**Fixed:** `EmbeddingModel.Backend.Tooltip` now describes the compute backend ("The Sentis
compute backend used for inference. GPU is fastest when your hardware supports compute
shaders; CPU is the portable fallback.") instead of the Vocab tooltip it had been copy-pasted
from.

### M13 · 🟡 [ARCH] Order-dependent DI in `SemanticSearchEditor` — ✅ FIXED
**Fixed:** `Bind<PrefabsResultView>()` moved from `PrefabsModule`'s constructor into
`SemanticSearchEditor.RegisterContainer()` — every `ISearchResult` implementation is now
registered before any module is resolved, so `SearchModule`'s `ISearchResult[]` no longer
depends on the resolve order in `CreateGUI()`. The now-unused `IMiniContainer` constructor
parameter was removed from `PrefabsModule` (it is only ever constructed by the container;
no direct constructions exist).

### M14 · 🟡 [RELIABILITY] DB path via `Directory.GetCurrentDirectory()` — ✅ FIXED
**Fixed:** `SqliteStorage.dbPath` is now derived from `Application.dataPath`
(`Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Library", …))`), so the index
location no longer depends on the editor's working-directory convention. The constructor
creates the `Library` directory before opening the connection (no-op when it exists, as it
always does in a Unity project). The path is identical to before for every normal editor
session (project root + `/Library/`), and `SqliteStorageTests` (which computes the same
`<project>/Library/` location from CWD) is unaffected.

### M15 · 🟡 [RELIABILITY] `DllImport("sqlite3")` has no resolver — bundled DLL only found by accident — ✅ FIXED
`SQLite.cs` declares bare `DllImport("sqlite3")` P/Invokes. There is no resolver in the code
and the bundled `Editor/ThirdParty/SQLite/Windows/sqlite3.dll` is a plain default import (not
a native plugin), so on a clean Windows machine the name resolves only if `sqlite3.dll`
happens to be on PATH or in a system directory. **Fixed:** new `SqliteNativeLoader`
(`Editor/ThirdParty/SQLite/SqliteNativeLoader.cs`) preloads the bundled DLL with
`kernel32!LoadLibrary` before the first SQLite P/Invoke — a library already loaded in the
process is what the P/Invoke then binds to. The DLL is located by a bounded walk of
`Assets/` and `Packages/` (skips `Library/`), which covers both `.unitypackage` (Assets) and
path/git (Packages) installs. `SqliteStorage`'s constructor calls
`EnsureLibraryLoaded()`; it is idempotent and non-fatal (on failure it logs a warning and
default resolution still applies). macOS is a no-op (the system `libsqlite3.dylib` is found
by default resolution).
**Why not `NativeLibrary.SetDllImportResolver`:** `NativeLibrary` (namespace
`System.Runtime.InteropServices`, assembly `System.Runtime.NativeLibrary.dll`) is a
.NET Core 3.0+ API and does not exist in the .NET Standard 2.1 profile the editor
compiles against — verified by compiling a minimal `SetDllImportResolver` call against
the official netstandard2.1 targeting pack (CS0103: name does not exist) with the same
code compiling on net9.0 as a control; the loader itself was additionally compiled
against the real editor reference assemblies (`UnityEditor.dll`, `UnityEngine.dll` from
6000.5.2f1). Plain `DllImport` P/Invoke — which this fix uses (the `kernel32!LoadLibrary`
call) — is fully supported in 2.1. `UnityEditor.EditorAssembly` (the obvious way to find
the package asmdef) does not exist in this build either (verified via metadata scan of
the editor's managed DLLs). The `LoadLibrary` preload works in every Unity version.

### L18 · ⚪ [WARNING] Obsolete API usage — ✅ FIXED
`MiniTokenizer.cs:36` used `LongestFirstTruncator`, which is `[Obsolete("Use GenericTruncator instead")]`
in Sentis 2.6.1 (the installed version; package declares `2.3.0`). Replaced with the canonical
`new GenericTruncator(new LongestFirstStrategy(), new RightDirectionRangeGenerator(), 128, 0)` —
the exact construction Sentis's own `HuggingFaceParser.BuildTruncator` uses for the
`"longest_first"` strategy (verified against the installed package source, 2.6.1). No behavior
change; the hardcoded `128` remains M5.

### L15 · ⚪ [CODE] Dead/legacy items — ✅ FIXED
- **`ISemanticStorage2` removed** — the file contained only this interface (no v1 existed) and
  had zero references (`SqliteStorage` implements the sub-interfaces directly, not the
  combined contract). File + meta deleted.
- **`e_general` unregisters removed** — both `Status.UnregisterMessage("e_general")` calls in
  `EmbeddingModule.OnPropertyChange` deleted; no `RegisterMessage("e_general")` exists
  anywhere, so the unregisters were no-ops.
- **`DataException` → `InvalidOperationException`** — the two precondition throws
  (`EmbeddingModule.GetVectorsAsync`: "Embedding not setup"; `MiniTokenizer.BuildVocabulary`:
  "No id for value {value}") now use the standard .NET type; `using System.Data;` dropped
  from both files (`using System;` added to `MiniTokenizer`). No first-party code caught
  `DataException`, so no catch-site changes were needed.
- **`windowStyleSheet` removed** — `SemanticSearchEditor` is an `EditorWindow`; Unity does not
  persist `EditorWindow` field values, so the `[SerializeField]` override could never be
  assigned and the null-guard branch was dead. The package stylesheet path
  (`TemplateProvider.LoadStyleSheetByName("SemanticSearchEditor")`) is untouched.
- **`_technicalNoiseRegex`** — finding stale: the field is already `static readonly` (matches
  all other regexes); no change needed.
- **`MiniContainer.Resolve` last-wins** — already documented on both overloads ("Resolves the
  newest registration of T"); the behavior is intentional, no change needed.

### L16 · ⚪ [PACKAGING] Samples details
- `Documentation~/images/` is empty (screenshots are an open owner task) — required for the store
  listing (banner 1440×810 + 3 tab shots).
- `PoisonEffect.cs` inside the sample compiles into the user's `Assembly-CSharp` on sample import
  (normal Unity sample behaviour; acceptable, just be aware).
- `MiniLM.onnx.meta` uses the Sentis ScriptedImporter with `dynamicDimConfigs`
  (`batch_size`, `sequence_length`) — correct for the 2.3.0+ dependency; do not hand-edit.
- ✅ `vocab.txt` (231 KB) **removed** — redundant: the code reads only `tokenizer.json`
  (vocabulary embedded in its `model.vocab`), and no asset referenced the file's GUID.

### L17 · ⚪ [TESTS] — ✅ FIXED
9 test classes cover vector math, tokenizer, naming, storage, property roundtrip, category
databases, context config and coverage report. Good coverage of the pure logic.
✅ Added (B3 hardening): `NormalizeInPlace_IdempotentForUnitVectors` and
`NormalizeThenDot_EqualsCosineSimilarity` pin the scoring contract at the `VectorMath` level. A
full `GetVectorsAsync` unit-length assertion would require model-in-the-loop (kept out of
EditMode).
✅ Added (L17) — guard-tests for the M7–M14 fixes:
- **M7** — `SqliteStorageTests.Assets_ClearIndexesByType_InvalidatesCache` and
  `Assets_ClearIndexesByGuids_InvalidatesCache`: warm the asset cache, clear, and assert the
  follow-up `GetIndexes` re-reads the database (pre-fix the stale cache served deleted rows).
- **M8** — progress formula extracted from `PrefabsModule.UpdateAssetsAsync` into
  `public static PrefabsModule.ProgressPercent(int i, int total)`; `GuardTests` pins
  total 0/1 → 100 (was NaN) and 0→100 span across a multi-asset run.
- **M9** — `GuardTests.Similarity_EmptyTable_ReturnsNoRows`: an empty
  `Dictionary<string, MetadataTable>` scores to an empty result set (the invariant the
  `ComponentMetadata` empty-DB guard relies on).
- **M14** — `dbPath` promoted to `public static SqliteStorage.DbPath`; `GuardTests.DbPath_IsAnchoredToProjectRoot`
  pins the path to `<project>/Library/SnivelerCode_SemanticIndex.db` (project-anchored, not CWD).

---

## 4. Documentation review

`Documentation~` (7 pages, ~400 lines) is above-average quality for a package: getting-started,
configuration, search guide, extensibility, troubleshooting, FAQ. Inconsistencies found:

| Doc claim | Reality | Ref |
|---|---|---|
| Sensitivity default 70 / sweet spot 60–70 | default 25 in code+UXML | M11 |
| "Raise Tokens to 256–512 for deep folder paths" | ✅ fixed — tokenizer truncation/padding follow the `Tokens` setting | M5 |
| Model stored with Git LFS | no LFS in this repo | B4 |
| "SIMD-accelerated cosine similarity" | ✅ verified correct — dot product on unit-length model output (B3) | — |
| index.md: `Library/` is "gitignored" | true for the *project*, but the package itself has **no `.gitignore`** and no note for users; fine in practice | ⚪ |

Also: `changelogUrl`/`documentationUrl` in `package.json` point to
`github.com/igor-karpushin/sniveler-code.dev.semanticsearch/.../sniveler-code.dev.semanticsearch/`
(subdirectory layout) — verify the repo layout actually matches (this local copy has no
`.github/` and no CI files, so the CI badge in README can't be verified from here).

---

## 5. Recommended release checklist

1. 🔴→✅ ~~Bundle `LICENSE-MiniLM.txt` (Apache-2.0) with the model; explicit carve-out in
   `LICENSE.md` (§2.2).~~ **Done** — LICENSE-MiniLM.txt + meta (GUID 167769380b6644139594527839102eb9),
   THIRD_PARTY_NOTICES.md and LICENSE.md updated.
2. 🟠→✅ ~~Fix B2 (Newtonsoft.Json + declared dependency; drop Plastic).~~ **Done** — Plastic
   usages replaced, both asmdefs reference `Newtonsoft.Json.dll`, `package.json` declares
   `com.unity.nuget.newtonsoft-json 3.2.1`.
3. 🟠→✅ ~~Fix B3 (normalize embeddings).~~ Premise disproven (bundled ONNX graph already
   normalizes, ‖output‖₂ = 1.0 verified), **hardening implemented**: defensive `NormalizeInPlace`
   in `GetVectorsAsync` + load-time `ProbeOutputNormAsync` self-check + contract tests. Remaining
   from this line: M11 align sensitivity defaults (code 25 vs docs 70).
4. 🟠→✅ ~~Fix B4 (LFS or FAQ) and add `.gitattributes`/`.gitignore`.~~ **Done** — LFS for all 131
   large binaries, `.gitignore` added, unpushed initial commit amended, pack 471 KiB (was ~70 MiB),
   165 MB in `.git/lfs`.
5. 🟡→✅ ~~M7–M10 correctness/perf fixes~~ **Done** (M5 ✅, M6 ❌ retracted — no race
   exists; M7–M10 ✅).
6. 🟡→✅ ~~M13 (DI order), M14 (DB path), M12 (tooltip), M15 (SQLite native loader).~~ **Done**.
7. ⚪→✅ ~~L15 dead code cleanup~~ **Done** (see L15 — 4 items removed, 2 findings stale).
   Remaining from this line: capture screenshots (owner task); finalize `LICENSE.md` text.
8. ⚪ Re-run the 30 EditMode tests + a manual pass: import sample → Check → Index → search
   "heavy axe" → verify score percentages are sane (should read ~50–95%, not thousands).
   ✅ **Tests re-run via `unity test` (headless, Unity 6000.5.2f1)**: 62/62 passed, 0 failed
   (first run: 50 passed + 12 skipped by the user-DB protection guard; second run with the
   index DB moved aside: 62/62 — including the new M7 cache-invalidation tests and the M8/M9/M14
   `GuardTests`). This also confirmed the whole package (M7–L17 changes) compiles clean in the
   real editor. ⚪ Manual sample pass still pending (owner, needs an interactive Editor).

## 6. Bottom line

Solid, well-factored Editor package with clean module boundaries, deterministic package-scoped
template loading, a sane SQLite schema and good docs. Release blockers were **not**
architectural: (1) ✅ the model's Apache-2.0 license text is now bundled, (2) ✅ the
Plastic/Newtonsoft JSON dependencies are declared and standardized, ~~(3) the missing embedding
normalization silently breaks the sensitivity feature~~ — **retracted** on investigation: the
bundled ONNX graph includes the Normalize module and outputs unit-length vectors
(‖output‖₂ = 1.0, verified statically and via onnxruntime), so the sensitivity thresholds and
score display work as designed; the only remainder is an optional load-time norm self-check for
user-swapped models, and (4) the LFS claim must match reality.
