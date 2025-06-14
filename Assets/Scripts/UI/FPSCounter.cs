using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour {
    public TextMeshProUGUI FPSText;
    private float _deltaTime;

    private void Start() {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
    }

    private void Update() {
        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f; // Smoothing
        var fps = 1.0f / _deltaTime;
        FPSText.text = $"{Mathf.Ceil(fps)} FPS";
    }
}