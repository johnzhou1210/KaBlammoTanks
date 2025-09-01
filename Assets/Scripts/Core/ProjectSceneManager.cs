using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProjectSceneManager : NetworkBehaviour
{
    public static ProjectSceneManager Instance;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    

    public void LoadArenaScene() {
        if (NetworkManager.Singleton.IsServer) {
            NetworkManager.SceneManager.LoadScene("ArenaScene", LoadSceneMode.Additive);
        }
    }
    
    
    
    
    
    
}
