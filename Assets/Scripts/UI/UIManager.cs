using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ShowWinScreen() {
        Debug.Log("Showing win screen");
    }

    public void ShowLoseScreen() {
        Debug.Log("Showing lose screen");
    }
    
}
