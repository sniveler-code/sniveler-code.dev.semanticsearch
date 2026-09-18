# State — SemanticSearch package review (task 1) + MiniLM licensing (task 2)

Done:
- Full package review written to REVIEW.md (284 lines, 6 sections: manifest, legal/licensing,
  code findings B1-B4/M5-M14/L15-L17, docs, release checklist, bottom line).
- Task 2 answered (REVIEW.md §2.2 + chat): all-MiniLM-L6-v2 is Apache-2.0 (verified via HF API,
  tag license:apache-2.0 + cardData.license), base nreimers/MiniLM-L6-H384-uncased is MIT.
  Legal to bundle (even in paid store product). Missing obligation: no bundled Apache-2.0 license
  text file (only a URL in THIRD_PARTY_NOTICES.md). Fix = add LICENSE-MiniLM.txt next to the model
  + explicit carve-out in LICENSE.md + provenance note.
- Verified installed com.unity.ai.inference 2.6.1 (in F:/Unity/chamele0n/work/SemanticSearch
  Library/PackageCache): deps = burst/collections/imageconversion/dt.app-ui/nuget.newtonsoft-json.
  NO plastic. Plastic DLL comes from com.unity.collab-proxy (Unity VCS default package).
  Sentis's own tokenizer sample uses Newtonsoft.Json, not the Plastic namespace.

Key code bugs found (details in REVIEW.md):
- B3: RETRACTED — investigated against the artifact. Bundled MiniLM.onnx graph tail is
  model.1 MeanWordsPooling → model.2 Normalize (Abs/Pow/ReduceSum/Pow/Clip/Div →
  'sentence_embedding'); onnxruntime run: ‖output‖₂ = 1.0 exactly (L=8,16). So dot product =
  cosine similarity; thresholds/%/0.4f cutoff all work as designed. Downgraded to 🟡
  robustness: code depends on hidden invariant (user-assigned Model slot; IsAssignableFrom checks
  only input names). Optional: load-time norm self-check or defensive NormalizeInPlace in
  GetVectorsAsync + unit-length test. REVIEW.md B3/M11/L17/docs-table/checklist/bottom-line all
  corrected.
- B3 hardening IMPLEMENTED: (b) MiniProcessor.GetVectorsAsync → vector.NormalizeInPlace() per
  output (idempotent for bundled model); (a) IEmbeddingProcessor.ProbeOutputNormAsync (16-token
  probe, RAW norm, -1 on failure) + EmbeddingModule.OnModelChanged (now async Task) registers
  advisory NormKey message when |norm-1|>0.01; VectorMathTests += IdempotentForUnitVectors +
  NormalizeThenDot_EqualsCosineSimilarity. CHANGELOG updated.
- B2: MiniTokenizer.cs + CategoryDatabaseTests.cs use Unity.Plastic.Newtonsoft.Json (undeclared);
  LocalTransformer.cs uses Newtonsoft.Json (undeclared). Fix: standardize on Newtonsoft + declare
  com.unity.nuget.newtonsoft-json 3.2.1.
- B4: FAQ claims Git LFS; repo has no .gitattributes/LFS, 90MB plain blob, .git = 153MB.
- M5: MiniTokenizer hardcodes trunc/pad 128 vs configurable Tokens 128-512.
- M6: Schedule + Task.Yield + PeekOutput — no WaitForCompletion → race.
- M7: ClearIndexes(AssetStorageType) doesn't invalidate _assetCache.
- M8: progress div-by-zero when total==1. M9: topTags[0] on empty DB. M10: per-component batch-1
  embeds, no caching. M11: sensitivity default 25 (code) vs 70 (docs). M12: Backend tooltip
  copy-paste. M13: DI order dependence (PrefabsResultView bind). M14: CWD-based DB path.

Follow-up (done in same session):
- vocab.txt removed from sample (redundant — code reads only tokenizer.json; no code refs,
  no asset GUID refs). Updated: README, getting-started, troubleshooting, THIRD_PARTY_NOTICES,
  LICENSE.md, LICENSE-MiniLM.txt header, CHANGELOG (Removed), REVIEW.md L16.

B2 (done in same session):
- MiniTokenizer.cs + CategoryDatabaseTests.cs: Unity.Plastic.Newtonsoft.Json.Linq → Newtonsoft.Json.Linq
- main asmdef: precompiledReferences += "Newtonsoft.Json.dll" (pattern from Unity.InferenceEngine.Editor.asmdef)
- test asmdef: Plastic dll precompiled ref → "Newtonsoft.Json.dll"
- package.json deps: + "com.unity.nuget.newtonsoft-json": "3.2.1" (installed: 3.2.2; matches Sentis minimum)
- Verified: zero Plastic refs remain; all JSON valid.

L18 (done in same session):
- MiniTokenizer.cs: LongestFirstTruncator (obsolete in Sentis 2.6.1) → GenericTruncator(new
  LongestFirstStrategy(), new RightDirectionRangeGenerator(), 128, 0) + Strategies using.
  Verified against installed package source (HuggingFaceParser does exactly this). Behavior
  unchanged; hardcoded 128 = still M5.

B4 (done in same session):
- Remote github.com/sniveler-code/... exists but EMPTY (no refs; origin/main gone; ls-remote
  blank) → safe to rewrite local pre-release history.
- .gitignore added (OS/IDE/Unity-generated). .gitattributes: *.onnx *.fbx *.png *.dll + 4
  extensionless SQLite MacOSX binaries → 131 files on LFS, all converted to pointers.
- Amended the unpushed initial commit (folded in B1/B2/B3/vocab/L18 work), reflog expire +
  gc --prune=now. Final: pack 471 KiB (was ~70 MiB), tracked 1.8 MB, .git/lfs 165 MB.
- Hashes: b0e2abb (orig) → 31152fa → 0ca872f → 319988f (final HEAD).
- FAQ needed no changes (its LFS wording is now true).

Next steps (if user wants): first push (git push -u origin main — uploads 165MB LFS), then
M5 (tokenizer 128 hardcode), M6 (Schedule race), M7-M14.

Model quantization restructure (same session):
- User decision: ship ONLY the quantized model; fp32 .onnx moves OUT of the package (repo =
  package root, so "outside the package" = models-source~/ tilde folder: not imported,
  not shipped, still in git+LFS as source of record).
- git mv MiniLM.onnx(+meta) -> models-source~/ ; models-source~/README.md added.
- .gitattributes += *.sentis (LFS).
- Docs updated: README (new "Embedding model" section — fp32 location = models-source~/ +
  Hugging Face; quantize script: ModelLoader.Load(ModelAsset) -> ModelQuantizer.QuantizeWeights
  -> ModelWriter.Save(path); sample/quickstart/config/limitations refs -> MiniLM_uint8.sentis),
  faq.md (size, supported models, LFS, "Why is the shipped model quantized?" entry rewritten),
  getting-started.md, configure.md, index.md.
- LICENSE-MiniLM.txt header: applicable work = MiniLM_uint8.sentis + tokenizer.json +
  models-source~/MiniLM.onnx; statement of changes = float32 ONNX conversion + uint8
  quantized derivative (Sentis ModelQuantizer).
- THIRD_PARTY_NOTICES.md: files line + Changes line added.
- API facts (installed Sentis 2.6.1): QuantizationType {Float16, Uint8}; ModelWriter.Save
  writes .sentis; ModelLoader.Load(string) reads .sentis ONLY — ONNX parsing is
  editor-importer-only (internal ONNXModelConverter) => user's script must load via ModelAsset
  (place the .onnx in the project first).
- PENDING user action: run the README script in the editor -> MiniLM_uint8.sentis (~24 MB) in
  Samples~/DemoContent/Medieval/Models/ -> I commit it (Unity generates the .meta) + final
  amend+gc. Then: M15 candidate = DllImport("sqlite3") has no resolver (Windows DLL path risk).

Final model state (user action, same session):
- User generated MiniLM_uint8.sentis in their editor: 22,652,944 B (~21.6 MB) + Unity-made
  .meta in Samples~/DemoContent/Medieval/Models/.
- models-source~/ REMOVED from the repo (user decision: no fp32 in repo at all). Docs
  rolled back: README/faq/License-header/Notices/Changelog now point to Hugging Face only
  for the full-precision version.
- NOTE: Tests/Editor/VectorMathTests.cs carries an UNCOMMITTED user edit (AreEqual(copy, v)
  without the 1e-6f delta — exact float equality; passes only if input is an exact unit
  vector). Left untouched, flagged to owner.
- Committed: .sentis (LFS) + doc rollbacks, amended.
- NEXT per REVIEW.md release checklist: M5 (tokenizer hardcoded 128 vs configurable Tokens).

M5 DONE:
- MiniTokenizer.Get(text, maxLength) — truncator + padding parameterized (was hardcoded 128).
- IEmbeddingProcessor.SetTokenizer(asset, maxLength); MiniProcessor passes through.
- EmbeddingModule: OnPropertyChange gains MaxLength branch (rebuild tokenizer); Model + Vocab
  branches pass _model.MaxLength.Value.
- MiniTokenizerTests: 4 call sites -> Get(Json, 128); NEW Encode_RespectsMaxLength
  (300 OOV words; assert 32/256 truncation+padding).
- REVIEW.md M5 -> FIXED (+ docs table row), CHANGELOG entry added, committed (amended).
- NEXT: M6 (Schedule + Task.Yield race in MiniProcessor.GetVectorsAsync/ProbeOutputNormAsync).

M6 RETRACTED (verified against installed Sentis 2.6.1 source):
- Worker has NO async API (no ScheduleAsync/WaitForCompletionAsync — the proposed fix
  referenced non-existent methods). Schedule(params) is synchronous: layers run inline; GPU
  path ends in ExecuteCommandBufferAndClear() = Graphics.ExecuteCommandBuffer(cb) + cb.Clear()
  (Runtime/Core/Backends/GPUCompute/GPUCompute.cs). PeekOutput = non-owning reference, valid
  until next Schedule/Dispose (no per-call lifetime issue). => No race; current code correct.
- await Task.Yield() = intentional UI courtesy (owner-confirmed): editor repaints between
  batches during multi-batch indexing. Kept; comments added in MiniProcessor (both call sites)
  pinning Schedule-synchronous + yield intent.
- REVIEW.md M6 -> RETRACTED + checklist updated. Committed. NEXT: M7 (stale asset cache after
  partial ClearIndexes).

FIRST PUSH DONE (user: "commit all and push"):
- Committed EVERYTHING: VectorMathTests.cs user edit (exact-equality AreEqual), REVIEW.md +
  REVIEW.md.meta, state-review.md + .meta. Working tree clean.
- git push -u origin main -> github.com/sniveler-code/sniveler-code.dev.semanticsearch:
  131 LFS objects (105 MB) uploaded, branch main live. HEAD = 116e788 (single initial commit,
  recovery chain: 393af01 -> 116e788).

Commit strategy change (user): SEPARATE COMMIT PER TASK — amending the initial commit is over
(repo is pushed). M7 is the first standalone commit.

M7 DONE (standalone commit + push):
- SqliteStorage.ClearIndexes(AssetStorageType) now does _assetCache = null (matches HashSet
  overload + SetIndexes: every mutation invalidates the cache).
- Guard test ClearIndexes_ByType_InvalidatesAssetCache (SqliteStorageTests; auto-skips when a
  user DB exists — CI/clean-project only, like the rest of that class).
- Note: type overload had no UI callers at fix time (public API, not on IAssetsStorage) —
  preventive fix for future "clear all of a type" callers.
- NEXT: M8 (progress division by zero in PrefabsModule.UpdateAssetsAsync).

M8 DONE (standalone commit + push):
- PrefabsModule.UpdateAssetsAsync: callback now uses total <= 1 ? 100f : i * 100f / (total - 1)
  (NaN fix; 0->100 mapping preserved for total >= 2; the original M8 suggestion i*100f/total
  would have shifted the scale — last asset never reaching 100%).
- NEXT: M9 (topTags[0] on empty Components database).

M9 DONE (standalone commit + push):
- ComponentMetadata.ProcessAsync: topTags.Length == 0 -> continue (skip component, not
  return — one empty database must not abort remaining components).
- NEXT: M10 (per-component embedding, batch size 1, no caching).

M10 DONE (standalone commit + push):
- ComponentMetadata.ProcessAsync two-pass: pass1 = standard tags + collect custom cleaned
  names; batch-embed unique names missing from new per-session _nameVectorCache (Dictionary
  string->float[]); pass2 = similarity per component from cached vectors.
- Output identical (deterministic model); M9 guard preserved in pass 2.
- NEXT: M11 (sensitivity defaults inconsistent: code 25 vs docs 70).

M11 DONE (standalone commit + push):
- Decision: KEEP code default 25 (recall-first; changing it would silently alter search
  behavior for users). Docs aligned: README (2 spots), configure.md, getting-started.md,
  troubleshooting.md (default 25 + semantics; 60–70 = stricter/precision guidance).
- UXML tooltip already correct (higher = more precise) — kept.
- Bonus stale-line fixes in troubleshooting.md: model field mentions .sentis + .onnx;
  removed "Prefabs/Audios tab" (Audios removed pre-release).
- NEXT: M12 (Backend tooltip copy-paste bug).

M12 DONE (standalone commit + push):
- EmbeddingModel.Backend.Tooltip -> real backend description (was the Vocab tooltip text).
- NEXT: M13 (order-dependent DI in SemanticSearchEditor).

M13 DONE (standalone commit + push):
- Bind<PrefabsResultView>() moved PrefabsModule ctor -> SemanticSearchEditor.RegisterContainer
  (with M13 comment). PrefabsModule ctor: IMiniContainer param removed (only container
  constructs it — verified no direct `new PrefabsModule` anywhere).
- Note: PrefabsResultView is the ONLY concrete ISearchResult (audio removed pre-release) —
  the search results tab IS the prefab rows view.
- NEXT: M14 (DB path via Directory.GetCurrentDirectory()).

M14 DONE (standalone commit + push):
- SqliteStorage.dbPath: CWD-based -> Application.dataPath-based (Path.GetFullPath,
  same <project>/Library/ location in every normal editor session); constructor now
  CreateDirectory(Library) before opening the connection; _connection field init moved
  into ctor.
- SqliteStorageTests unaffected (computes the same path from CWD, equal in editor/CI).
- NEXT: M15 (candidate — DllImport("sqlite3") has no resolver on Windows; M5-M14 done,
  remaining: M15 + L15 dead code + L16/L17 packaging/test items).

Paid/store version (user decision):
- Install is now .unitypackage ONLY (no git URL in user-facing docs).
- README: CI badge removed; Quick Start step 1 = Assets → Import Package → Custom Package
  (.unitypackage from store); step 2 = sample content ships inside the package file (PM
  sample import as fallback).
- getting-started.md: Option A/B (git/disk) -> .unitypackage; "Import the sample" ->
  "Locate the sample assets".
- faq.md: bug report -> store support section; "installed via git URL" -> "imported the
  package but the sample model is missing".
- index.md install line; package.json: changelogUrl/documentationUrl REMOVED (private repo),
  sample description ~90MB ONNX -> ~22 MB uint8 sentis build. CHANGELOG entries added.
- Kept: company footer github.com/snivelercode (brand link, not the repo).
- Owner note: create the .unitypackage from the Editor (right-click the package root folder
  -> Export Package) so Samples~/, Documentation~/, legal files and package.json are included.
