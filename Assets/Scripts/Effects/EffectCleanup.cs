using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class EffectCleanup : NetworkBehaviour {
    [SerializeField] float cleanupTime = 1f;

    void Start() {
        if (IsServer) {
            StartCoroutine(Cleanup());
        }
    }

    private IEnumerator Cleanup() {
        yield return new WaitForSecondsRealtime(cleanupTime);
        Destroy(gameObject);
    }
}
