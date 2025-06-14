using UnityEngine;

public class CheckGraphicsBackend : MonoBehaviour {
    private void Start() {
        Debug.Log("Graphics Device: " + SystemInfo.graphicsDeviceType);
        Debug.Log("Operating System: " + SystemInfo.operatingSystem);
    }
}