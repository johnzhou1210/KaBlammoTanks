using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Netcode;

public enum BattleResult {
    PLAYER_VICTORY,
    ENEMY_VICTORY,
    STALEMATE,
    IN_PROGRESS
}

public class TankBattleManager : NetworkBehaviour {
    private BattleResult _battleStatus = BattleResult.IN_PROGRESS;
    [SerializeField] private GameObject battleStatusPopup, countdownPopup;
    [SerializeField] private Volume postProcessingVolume;
    [SerializeField] private VolumeProfile winVolumeProfile, loseVolumeProfile;
    
    private void OnEnable() {
        TankBattleDelegates.OnCheckIfBattleIsOver += CheckIfBattleOver;
    }

    private void OnDisable() {
        TankBattleDelegates.OnCheckIfBattleIsOver -= CheckIfBattleOver;
    }

    private void CheckIfBattleOver() {
        if (_battleStatus != BattleResult.IN_PROGRESS) return; // Prevent multiple calls if battle is already over
        // If battle over, trigger battle end
        _battleStatus = GetBattleStatus();
        if (_battleStatus != BattleResult.IN_PROGRESS) {
            print(_battleStatus);
            Invoke(nameof(EndBattle), 1f);
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
        battleStatusPopup.SetActive(true);
        TextMeshProUGUI battleStatusText = battleStatusPopup.GetComponentInChildren<TextMeshProUGUI>();
        Time.timeScale = 0f;
        switch (_battleStatus) {
            case BattleResult.PLAYER_VICTORY:
                battleStatusText.text = "VICTORY";
                postProcessingVolume.profile = winVolumeProfile;
                break;
            case BattleResult.ENEMY_VICTORY:
                battleStatusText.text = "DEFEAT";
                postProcessingVolume.profile = loseVolumeProfile;
                break;
            case BattleResult.STALEMATE:
                battleStatusText.text = "STALEMATE";
                break;
            default:
                Debug.LogError("Invalid battle status stored in _battleStatus variable!");
                break;
        }
        
    }

    private void Start() {
        StartCoroutine(BattleStartCountdown());
    }

    private IEnumerator StartBattle() {
        yield return null;
    }

    private IEnumerator BattleStartCountdown() {
        int countdownTime = 3;
        countdownPopup.SetActive(true);
        TextMeshProUGUI battleCountdownText = countdownPopup.GetComponentInChildren<TextMeshProUGUI>();
        for (int i = countdownTime; i > 0; i--) {
            AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/SFX/CinematicHit"), 1f);
            battleCountdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/SFX/CannonFire"), 1f);
        battleCountdownText.text = "FIGHT!";
        yield return new WaitForSeconds(1f);
        countdownPopup.SetActive(false);
        TankBattleDelegates.InvokeOnInitTanks();
        
        
    }

}
