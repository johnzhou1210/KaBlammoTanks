using System;
using UnityEngine;

public enum BattleResult {
    PLAYER_VICTORY,
    ENEMY_VICTORY,
    STALEMATE,
    IN_PROGRESS
}

public class TankBattleManager : MonoBehaviour {
    private BattleResult _battleStatus = BattleResult.IN_PROGRESS;
    private void OnEnable() {
        TankBattleDelegates.CheckIfBattleIsOver += CheckIfBattleOver;
    }

    private void OnDisable() {
        TankBattleDelegates.CheckIfBattleIsOver -= CheckIfBattleOver;
    }

    private void CheckIfBattleOver() {
        if (_battleStatus != BattleResult.IN_PROGRESS) return; // Prevent multiple calls if battle is already over
        // If battle over, trigger battle end
        _battleStatus = GetBattleStatus();
        if (_battleStatus != BattleResult.IN_PROGRESS) {
            print(_battleStatus);
            EndBattle();
        }
    }

    private BattleResult GetBattleStatus() {
        // Get the health of both tanks
        int playerTankHealth = TankDelegates.GetTankHealthById?.Invoke(0) ?? 0;
        int enemyTankHealth = TankDelegates.GetTankHealthById?.Invoke(1) ?? 0;
        if (playerTankHealth == 0 && enemyTankHealth == 0) return BattleResult.STALEMATE;
        if (enemyTankHealth == 0) return BattleResult.PLAYER_VICTORY;
        if (playerTankHealth == 0) return BattleResult.ENEMY_VICTORY;
        return BattleResult.IN_PROGRESS;
    }

    private void EndBattle() {
        switch (_battleStatus) {
            case BattleResult.PLAYER_VICTORY:
                break;
            case BattleResult.ENEMY_VICTORY:
                break;
            case BattleResult.STALEMATE:
                break;
            default:
                Debug.LogError("Invalid battle status stored in _battleStatus variable!");
                break;
        }
    }

}
