using System;
using UnityEngine;

public enum GameSessionType {
   SINGLEPLAYER,
   MULTIPLAYER,
}

public class GameSessionManager : MonoBehaviour
{
   public static GameSessionManager Instance;
   
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
      GameSessionType = gameSessionType;
   }
   
   
   
}
