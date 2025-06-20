using UnityEditor;

public static class ManualRefreshShortcut {
    [MenuItem("Tools/Refresh Assets %s")]
    public static void RefreshAssets() {
        AssetDatabase.Refresh();
    }
}