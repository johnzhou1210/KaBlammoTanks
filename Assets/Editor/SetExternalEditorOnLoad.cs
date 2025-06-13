using UnityEditor;

#if UNITY_EDITOR_LINUX
[InitializeOnLoad]
public class SetExternalEditorOnLoad {
    static SetExternalEditorOnLoad() {
        // Set to your desired editor path
        EditorPrefs.SetString("kScriptsDefaultApp", "/home/johnzhou/bin/rider-fixed");
    }
}
#endif