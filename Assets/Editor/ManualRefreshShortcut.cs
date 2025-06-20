using UnityEditor;

public static class ManualRefreshShortcut {
    [MenuItem("Tools/Refresh Assets %r")]
    public static void RefreshAssets() {
        AssetDatabase.Refresh();
    }
}