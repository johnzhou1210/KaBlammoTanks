using System;
using Unity.Netcode;
using UnityEngine;

public enum GameState {
    Lobby,
    Playing,
    Results
}

public class GameManager : NetworkBehaviour {
    public static GameManager Instance;
    
    private NetworkVariable<GameState> state = new NetworkVariable<GameState>(GameState.Lobby);

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn() {
        if (IsHost) {
            state.Value = GameState.Lobby;
        }
        state.OnValueChanged += OnGameStateChanged;
    }

    public override void OnNetworkDespawn() {
        state.OnValueChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState oldState, GameState newState) {
        Debug.Log($"OnGameStateChanged: {oldState} -> {newState}");
    }
    
    # region Match Flow

    [ServerRpc(RequireOwnership = false)]
    public void StartMatchServerRpc() {
        if (state.Value == GameState.Lobby) {
            state.Value = GameState.Playing;

            NetworkSceneManager.Instance.LoadGameScene();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndMatchServerRpc(ulong winnerId) {
        if (state.Value == GameState.Playing) {
            state.Value = GameState.Results;
            
            Debug.Log($"Player {winnerId} won the match!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetToLobbyServerRpc() {
        if (state.Value == GameState.Results) {
            state.Value = GameState.Lobby;
            NetworkSceneManager.Instance.LoadLobbyScene();
        }
    }
    
    # endregion


}
