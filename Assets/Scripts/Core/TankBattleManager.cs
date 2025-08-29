using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Netcode;
using Random = UnityEngine.Random;

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
        StartCoroutine(EndBattleCutscene(winningClientId));
    }

    [ClientRpc]
    private void FocusHostCameraClientRpc(ClientRpcParams clientRpcParams = default) {
        ArenaUIManager.Instance.FocusHostCamera();
    }
    
    [ClientRpc]
    private void FocusHosteeCameraClientRpc(ClientRpcParams clientRpcParams = default) {
        ArenaUIManager.Instance.FocusHosteeCamera();
    }

    [ClientRpc]
    private void HideBattleUIClientRpc(ClientRpcParams clientRpcParams = default) {
        ArenaUIManager.Instance.HideBattleUI();
    }
    
    private IEnumerator EndBattleCutscene(ulong? winningClientId) {
        if (winningClientId == null) {
            throw new Exception("WinningClient is null!");
        }
        
        // Freeze time
        if (IsClient) {
            Time.timeScale = 0;
        }
        
        // Disable battle UI (e.g. ammo slot destinations, and ammo shop)
        HideBattleUIClientRpc();
        
        ulong killedTank = winningClientId.Value == 0 ? (ulong) 1 : 0;
        // Focus camera onto killed tank
        if (killedTank == 0) {
            FocusHostCameraClientRpc();
        } else {
            FocusHosteeCameraClientRpc();
        } 
        
        StartCoroutine(PlayDeathAnim(killedTank));
        
        
        yield return new WaitForSecondsRealtime(10f);
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
            ArenaUIManager.Instance.ShowWinScreen();
            postProcessingVolume.profile = winVolumeProfile;
        } else {
            ArenaUIManager.Instance.ShowLoseScreen();
            postProcessingVolume.profile = loseVolumeProfile;
        }
    }

    [ClientRpc]
    private void PlayExplosionSoundClientRpc(Vector3 position) {
        AudioManager.Instance.PlaySFXAtPoint(position, Resources.Load<AudioClip>("Audio/SFX/Explosion"));
    }

    private IEnumerator PlayDeathAnim(ulong killedTankId) {
        TanksManager tanksManager = GameObject.FindWithTag("TanksManager").GetComponent<TanksManager>();
        GameObject killedTank = killedTankId == 0 ? tanksManager.GetHostTankGO() : tanksManager.GetHosteeTankGO();
        GameObject explosionPrefab = Resources.Load<GameObject>("Prefabs/VFX/ExplosionEffect");
        for (int i = 0; i < 32; i++) {
            GameObject explosionInstance = Instantiate(explosionPrefab, killedTank.transform.position + new Vector3(Random.Range(-1f,1f) * 1.5f, Random.Range(-1f,1f) * 1.5f, 0f), Quaternion.identity);
            NetworkObject explosionNetworkObject = explosionInstance.GetComponent<NetworkObject>();
            explosionNetworkObject.Spawn();
            PlayExplosionSoundClientRpc(explosionInstance.transform.position);
            yield return new WaitForSecondsRealtime(Random.Range(0.05f, 0.34f));
        }
        
        yield return null;
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
