using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartManager : MonoBehaviour
{
    private IEnumerator ResubscribeToNetworkManager() {
        yield return new WaitUntil((() => NetworkManager.Singleton != null));
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        
    }
    
    private void OnEnable() {
        StartCoroutine(ResubscribeToNetworkManager());
        Debug.Log("Resubscribed to network manager events via GameStartManager OnEnable");

    }

    private void OnDisable() {
        if (NetworkManager.Singleton != null) {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
            Debug.Log("Unsubscribed to network manager events via GameStartManager OnDisable");
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
        int currentPlayers = NetworkManager.Singleton.ConnectedClients.Count;
        if (currentPlayers >= 2) {
            Debug.Log($"Rejecting connection from {request.ClientNetworkId}, server full.");
            response.Approved = false;
            response.CreatePlayerObject = false;
        } else {
            response.Approved = true;
            response.CreatePlayerObject = true;
        }
    }
    
    private void OnClientConnected(ulong clientId) {
        Debug.Log($"CLIENT CONNECTED: {clientId}");
        TryStartGame();
    }

    private void TryStartGame() {
       

        if (NetworkManager.Singleton.ConnectedClients.Count == 2) {
            LocalSceneManager.Instance.SetSoloPlay(false);
            LocalSceneManager.Instance.UnloadTitleScene();
            Debug.Log($"Trying to start game with 2 players");
            // Make the remote client stop listening
            // if (NetworkManager.Singleton.IsConnectedClient && !NetworkManager.Singleton.IsHost) {
            //     LanDiscovery.Instance.StopListening();
            // }
            if (NetworkManager.Singleton.IsClient) {
                Debug.LogWarning("Stopped listening via GameStartManager TryStartGame");
                LanDiscovery.Instance.StopListening();
            }

            if (NetworkManager.Singleton.IsHost) {
                NetworkManager.Singleton.SceneManager.LoadScene("ArenaScene", LoadSceneMode.Additive);
            }
            
            // Do not stop broadcasting until battle is over! If you stop broadcasting, everything breaks...
            
        }
    }
    
    
    
    
}
