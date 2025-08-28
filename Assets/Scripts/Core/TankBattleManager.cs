using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Netcode;
using UnityEngine.PlayerLoop;

public enum BattleResult {
    VICTORY,
    DEFEAT,
    STALEMATE,
    IN_PROGRESS
}

public class TankBattleManager : NetworkBehaviour {
    public NetworkVariable<BattleResult> BattleStatus = new NetworkVariable<BattleResult>(BattleResult.IN_PROGRESS);
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
        if (BattleStatus.Value != BattleResult.IN_PROGRESS) return; // Prevent multiple calls if battle is already over
        // If battle over, trigger battle end
        ulong? winnerId = GetWinningClientId();
        if (winnerId != null) {
            EndBattle(winnerId);
        }
    }

    private ulong? GetWinningClientId() {
        // Get the health of both tanks
        TankController hostTankController = TankDelegates.GetHostTankController?.Invoke();
        TankController hosteeTankController = TankDelegates.GetHosteeTankController?.Invoke();
        int hostTankHealth = hostTankController!.TankHealth.Value;
        int hosteeTankHealth = hosteeTankController!.TankHealth.Value;
        if (hosteeTankHealth == 0) return hostTankController!.OwnerClientId;
        if (hostTankHealth == 0) return hosteeTankController!.OwnerClientId;
        return null;
    }

    public void EndBattle(ulong? winningClientId) {
        if (winningClientId == null) return;
        foreach (var clientPair in NetworkManager.Singleton.ConnectedClients) {
            ulong clientId = clientPair.Key;
            if (clientId == winningClientId) {
                ShowResultClientRpc(BattleResult.VICTORY, new ClientRpcParams {
                    Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } }
                });
            } else {
                ShowResultClientRpc(BattleResult.DEFEAT, new ClientRpcParams {
                    Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } }
                });
            }
        }
    }

    [ClientRpc]
    private void ShowResultClientRpc(BattleResult result, ClientRpcParams clientRpcParams = default) {
        if (result == BattleResult.VICTORY) {
            UIManager.Instance.ShowWinScreen();
            postProcessingVolume.profile = winVolumeProfile;
        } else {
            UIManager.Instance.ShowLoseScreen();
            postProcessingVolume.profile = loseVolumeProfile;
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
