using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class SetExternalEditorOnLoad
{
    static SetExternalEditorOnLoad()
    {
        // Set to your desired editor path
        EditorPrefs.SetString("kScriptsDefaultApp", "/home/johnzhou/bin/rider-fixed");
    }
}
