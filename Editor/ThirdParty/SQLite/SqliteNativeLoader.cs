using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SnivelerCode.SemanticSearch.Editor.Core.Storage.SQLite
{
    /// <summary>
    /// Makes the bare DllImport("sqlite3") declarations in SQLite.cs resolve to
    /// this package's bundled sqlite3.dll on Windows (REVIEW M15). The .NET
    /// Standard 2.1 profile used by the editor has no DllImport-resolver API
    /// (NativeLibrary is .NET Core 3.0+ only), so the library is preloaded with
    /// LoadLibrary instead — once a native library is loaded in the process, the
    /// subsequent P/Invoke binds to it. On macOS nothing is done: "sqlite3"
    /// resolves to the system libsqlite3.dylib there.
    /// </summary>
    internal static class SqliteNativeLoader
    {
        private static bool _ensured;

        /// <summary>Preloads the bundled SQLite library. Idempotent, non-fatal.</summary>
        public static void EnsureLibraryLoaded()
        {
            if (_ensured)
            {
                return;
            }

            _ensured = true;

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            string path = FindBundledLibrary();
            if (path == null)
            {
                return;
            }

            if (LoadLibrary(path) == IntPtr.Zero)
            {
                // Non-fatal: default resolution (PATH / system directories) still applies.
                Debug.LogWarning($"AI Semantic Search: could not load bundled SQLite library {path}.");
            }
        }

        /// <summary>
        /// Locates the bundled sqlite3.dll on disk. It may live under Assets/
        /// (.unitypackage import) or under Packages/ (path or git reference);
        /// Library/ is never scanned because it is generated.
        /// </summary>
        private static string FindBundledLibrary()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            foreach (string root in new[] { "Assets", "Packages" })
            {
                string dir = Path.Combine(projectRoot, root);
                if (!Directory.Exists(dir))
                {
                    continue;
                }

                foreach (string file in Directory.GetFiles(dir, "sqlite3.dll", SearchOption.AllDirectories))
                {
                    return file;
                }
            }

            return null;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);
    }
}
