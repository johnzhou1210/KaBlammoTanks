// #if UNITY_EDITOR_LINUX
//
// using System.IO;
// using UnityEditor;
// using UnityEngine;
//
// [InitializeOnLoad]
// public static class AutoRecompileWatcher {
//     private static readonly string[] extensions = { "*.cs", "*.shader", "*.uxml", "*.asmdef" };
//     private static readonly FileSystemWatcher[] watchers = new FileSystemWatcher[extensions.Length];
//
//     static AutoRecompileWatcher() {
//         EditorApplication.update += InitWatchers;
//     }
//
//     private static void InitWatchers() {
//         for (int i = 0; i < extensions.Length; i++) {
//             if (watchers[i] != null) continue;
//
//             string ext = extensions[i];
//             FileSystemWatcher watcher = new(Application.dataPath, ext) {
//                 IncludeSubdirectories = true,
//                 EnableRaisingEvents = true,
//                 NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
//             };
//
//             watcher.Changed += OnChanged;
//             watcher.Created += OnChanged;
//             watcher.Renamed += OnChanged;
//
//             watchers[i] = watcher;
//         }
//     }
//
//     private static void OnChanged(object sender, FileSystemEventArgs e) {
//         EditorApplication.delayCall += () => {
//             if (EditorApplication.isPlayingOrWillChangePlaymode ||
//                 EditorApplication.isCompiling ||
//                 EditorApplication.isUpdating) return;
//
//             AssetDatabase.Refresh();
//         };
//     }
// }
//
// #endif

