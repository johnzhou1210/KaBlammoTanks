using System;
using UnityEngine;

public enum GameSessionType {
   SINGLEPLAYER,
   MULTIPLAYER,
}

public class GameSessionManager : MonoBehaviour
{
   public static GameSessionManager Instance;
   [SerializeField] private LanDiscovery lanDiscovery;
   
   public GameSessionType GameSessionType {get; private set; }
   private void Awake() {
      if (Instance != null && Instance != this) {
         Destroy(gameObject);
      } else {
         Instance = this;
         DontDestroyOnLoad(this);
      }
   }

   public void SetGameSessionType(GameSessionType gameSessionType) {
      Debug.Log($"Setting game session type to {gameSessionType}");
      GameSessionType = gameSessionType;
   }

   public void SetLanDiscoveryActive(bool active) {
      lanDiscovery.gameObject.SetActive(active);
   }
   
   
   
}
