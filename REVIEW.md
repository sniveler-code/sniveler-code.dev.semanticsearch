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

### M8 · 🟡 [CORRECTNESS] Division by zero in Check progress
`PrefabsModule.UpdateAssetsAsync`: `callback?.Invoke(path, i * 100f / (total - 1))` → `0/0 = NaN`
when exactly one asset is new (ProgressBar NaN). Use `total == 0 ? 0f : i * 100f / total`.

### M9 · 🟡 [CORRECTNESS] `topTags[0]` on empty Components database
`ComponentMetadata.ProcessAsync`: if `DatabaseType.Components` has no rows, `results` is empty and
`topTags[0]` throws `IndexOutOfRangeException` (only masked because AutoBake runs first in the
normal Index flow). Guard `topTags.Length == 0` → return.

### M10 · 🟡 [PERF] Per-component embedding, batch size 1, no caching
`ComponentMetadata` embeds each custom component name individually (`GetVectorsAsync(words)` with
1 word) for every prefab, and the same script type is re-embedded per asset. For a project with
hundreds of prefabs sharing scripts this is the dominant indexing cost on the CPU backend.
**Fix:** collect all unique component names per run, embed once in batches, cache
`name → vector` for the session.

### M11 · 🟡 [DOC/UX] Sensitivity defaults inconsistent
- Code + UXML default: **25** (`SearchModel.Sensitivity = 25`, `SliderInt value="25"`).
- README + configure.md: "default 70", "60–70 is the sweet spot".
- UXML tooltip: "Higher values (70%+) return more precise matches" (threshold semantics).
With model outputs unit-length (B3 verified), scores are true cosine similarities: 25 is a loose
threshold, 70 a strict one. Pick one default, make code, UXML and docs agree, and state the
semantics (higher = stricter / fewer results).

### M12 · 🟡 [DOC/UX] `Backend` tooltip copy-paste bug
`EmbeddingModel.Backend.Tooltip` says "The vocabulary file used by the BERT tokenizer to encode
the text." — wrong property's tooltip.

### M13 · 🟡 [ARCH] Order-dependent DI in `SemanticSearchEditor`
`PrefabsModule`'s constructor calls `container.Bind<PrefabsResultView>()`; `SearchModule`'s
constructor resolves `ISearchResult[]`. This only works because `Resolve<PrefabsModule>()` is
called before `Resolve<SearchModule>()` in `CreateGUI()`. Move the `Bind<PrefabsResultView>()`
into `RegisterContainer()` so the registration no longer depends on call order.

### M14 · 🟡 [RELIABILITY] DB path via `Directory.GetCurrentDirectory()`
`SqliteStorage.dbPath` = `CWD + "/Library/SnivelerCode_SemanticIndex.db"`. In the Editor CWD is the
project root today, but that's an implicit contract. Prefer
`Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Library", "SnivelerCode_SemanticIndex.db"))`
and `Directory.CreateDirectory` on the `Library` segment for safety.

### L18 · ⚪ [WARNING] Obsolete API usage — ✅ FIXED
`MiniTokenizer.cs:36` used `LongestFirstTruncator`, which is `[Obsolete("Use GenericTruncator instead")]`
in Sentis 2.6.1 (the installed version; package declares `2.3.0`). Replaced with the canonical
`new GenericTruncator(new LongestFirstStrategy(), new RightDirectionRangeGenerator(), 128, 0)` —
the exact construction Sentis's own `HuggingFaceParser.BuildTruncator` uses for the
`"longest_first"` strategy (verified against the installed package source, 2.6.1). No behavior
change; the hardcoded `128` remains M5.

### L15 · ⚪ [CODE] Dead/legacy items
- `ISemanticStorage2` — never referenced.
- `EmbeddingModule` unregisters message key `"e_general"` which is never registered.
- `using System.Data;` in `EmbeddingModule`/`MiniProcessor` just for `DataException` — prefer
  `InvalidOperationException`.
- `[SerializeField] private StyleSheet windowStyleSheet;` in `SemanticSearchEditor` — documented
  as legacy; fine, but consider removing in 1.1 if nothing uses it.
- `NamingUtility._technicalNoiseRegex` is non-static (all other regexes are static).
- `MiniContainer.Resolve` uses `list.Last()` (last registration wins) — document or enforce.

### L16 · ⚪ [PACKAGING] Samples details
- `Documentation~/images/` is empty (screenshots are an open owner task) — required for the store
  listing (banner 1440×810 + 3 tab shots).
- `PoisonEffect.cs` inside the sample compiles into the user's `Assembly-CSharp` on sample import
  (normal Unity sample behaviour; acceptable, just be aware).
- `MiniLM.onnx.meta` uses the Sentis ScriptedImporter with `dynamicDimConfigs`
  (`batch_size`, `sequence_length`) — correct for the 2.3.0+ dependency; do not hand-edit.
- ✅ `vocab.txt` (231 KB) **removed** — redundant: the code reads only `tokenizer.json`
  (vocabulary embedded in its `model.vocab`), and no asset referenced the file's GUID.

### L17 · ⚪ [TESTS]
9 test classes (~30 EditMode tests) cover vector math, tokenizer, naming, storage, property
roundtrip, category databases, context config and coverage report. Good coverage of the pure
logic. ✅ Added (B3 hardening): `NormalizeInPlace_IdempotentForUnitVectors` and
`NormalizeThenDot_EqualsCosineSimilarity` pin the scoring contract at the `VectorMath` level. A
full `GetVectorsAsync` unit-length assertion would require model-in-the-loop (kept out of
EditMode). Still missing: guard-tests for M7/M8-level storage invariants.

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
5. 🟡 M7–M10 correctness/perf fixes (M5 ✅ fixed; M6 ❌ retracted — no race exists).
6. 🟡 M13 (DI order), M14 (DB path), M12 (tooltip).
7. ⚪ L15 dead code cleanup; capture screenshots (owner task); finalize `LICENSE.md` text.
8. Re-run the 30 EditMode tests + a manual pass: import sample → Check → Index → search
   "heavy axe" → verify score percentages are sane (should read ~50–95%, not thousands).

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
