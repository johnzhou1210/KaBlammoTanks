using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartManager : MonoBehaviour
{
    private IEnumerator SubscribeToNetworkManager() {
        yield return new WaitUntil((() => NetworkManager.Singleton != null));
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }
    
    private void OnEnable() {
        StartCoroutine(SubscribeToNetworkManager());

    }

    private void OnDisable() {
        if (NetworkManager.Singleton != null) {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId) {
        Debug.Log($"Client connected: {clientId}");
        TryStartGame();
    }

    private void TryStartGame() {
       

       
        
        if (NetworkManager.Singleton.ConnectedClients.Count == 2) {
            LocalSceneManager.Instance.UnloadTitleScene();
            Debug.Log($"Trying to start game with 2 players");
            ProjectSceneManager.Instance.LoadArenaScene();
        }
    }
    
    
    
}
