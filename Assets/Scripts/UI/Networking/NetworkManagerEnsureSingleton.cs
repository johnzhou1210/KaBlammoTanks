using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkManagerEnsureSingleton : MonoBehaviour
{
    public static NetworkManagerEnsureSingleton Instance;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }
}
