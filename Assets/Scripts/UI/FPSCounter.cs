using System;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour {
   public TextMeshProUGUI FPSText;
   float _deltaTime;

   void Start() {
      QualitySettings.vSyncCount = 0;
      Application.targetFrameRate = 60;
   }

   void Update() {
      _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f; // Smoothing
      float fps = 1.0f / _deltaTime;
      FPSText.text = $"{Mathf.Ceil(fps)} FPS";
   }
   
}
