using System;
using UnityEngine;

public class ConsolePersist : MonoBehaviour {
    public static ConsolePersist Instance;
    private void Awake() {
        if (Instance && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
