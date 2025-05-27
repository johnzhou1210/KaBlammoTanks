using System;
using UnityEngine;

public class EffectCleanup : MonoBehaviour {
    [SerializeField] float cleanupTime = 1f;

    void Start() {
        Invoke(nameof(Cleanup), cleanupTime);
    }

    private void Cleanup() {
        Destroy(gameObject);
    }
}
