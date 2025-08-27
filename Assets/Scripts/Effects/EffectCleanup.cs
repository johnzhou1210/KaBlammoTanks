using System;
using Unity.Netcode;
using UnityEngine;

public class EffectCleanup : NetworkBehaviour {
    [SerializeField] float cleanupTime = 1f;

    void Start() {
        if (IsServer) {
            Invoke(nameof(Cleanup), cleanupTime);
        }
    }

    private void Cleanup() {
        Destroy(gameObject);
    }
}
