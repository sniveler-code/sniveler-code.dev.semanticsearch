# Screenshots — how to capture (owner task)

The `images/` folder must contain **real screenshots** taken in a Unity GUI session
(batch mode cannot render the window). Capture them on Unity 6000.5.2f1 with the sample
imported and a small index built, then commit the PNGs here and mirror them to
`docs/screenshots/` in the repo.

## Required shots (PNG, 16:9, 1280×720 minimum; banner 1440×810)

1. `window-search.png` — full window, **Search** tab with results for "heavy axe"
   (status bar visible).
2. `tab-embedding.png` — **Embedding** tab: model + tokenizer.json assigned, Backend GPU.
3. `tab-prefabs.png` — **Prefabs** tab with Indexed count and extractor settings visible.
4. `store-banner.png` — wide banner for the Asset Store listing (1440×810).

## Steps

1. Open the project, import the sample, open `Window > SnivelerCode > Semantic Search`.
2. Set up the model in **Embedding**, run **Check/Index** in the **Prefabs** tab.
3. Run a demo search with results visible.
4. Use your OS screenshot tool (or a capture package); keep the window at 1280×720+.
5. Save into `sniveler-code.dev.semanticsearch/Documentation~/images/` and copy to
   `docs/screenshots/`.

## Tips

- Use the default editor theme (both themes ok; pick one and stay consistent).
- Close unrelated windows/dockers; enable "Maximize" (top-right) for the tool window.
- Crop pixels of the OS taskbar if present.