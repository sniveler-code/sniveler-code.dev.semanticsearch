# MIT License

Copyright (c) 2026 SnivelerCode (igor-karpushin)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

## Exceptions — bundled third-party content

This license does **not** apply to the third-party components listed in
`THIRD_PARTY_NOTICES.md`. Those components keep their own licenses, which take
precedence over this file:

- **AI model and tokenizer files** — `Samples~/DemoContent/Medieval/Models/MiniLM.onnx`,
  `Samples~/DemoContent/Medieval/Data/tokenizer.json` (and the bundled license file
  `Samples~/DemoContent/Medieval/Models/LICENSE-MiniLM.txt`):
  **Apache-2.0** (sentence-transformers / UKP Lab, pretrained base model
  nreimers/MiniLM-L6-H384-uncased, MIT). Full license text: see the bundled
  `LICENSE-MiniLM.txt`. These files remain under Apache-2.0 in any
  redistribution of this package, including commercial and Asset Store
  distributions.
- **SQLite-net** — `Editor/ThirdParty/SQLite/**`: **MIT**
  (see the bundled `Editor/ThirdParty/SQLite/LICENSE.txt`).

---

**NOTE FOR THE RELEASE OWNER:** the body of this license above applies to the
package's own source code (`Editor/**`, excluding `Editor/ThirdParty/**` and the
samples' third-party model files covered by the Exceptions section). If this
package is published as a paid asset on the Unity Asset Store, you may want to
replace it with the Asset Store EULA or a proprietary license instead — keep the
Exceptions section intact in that case, since third-party license terms cannot
be changed by the package license. Confirm before shipping. Full third-party
details are in `THIRD_PARTY_NOTICES.md`.