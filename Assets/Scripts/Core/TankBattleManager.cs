using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine.SceneManagement;
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
    [SerializeField] private Transform airfieldTransform;
    private Coroutine _endBattleCutscene;
    private void OnEnable() {
        TankBattleDelegates.OnCheckIfBattleIsOver += CheckIfBattleOver;
        if (NetworkManager.Singleton != null) {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
        TankDelegates.GetHosteeId = GetHosteeId;
    }

    private void OnDisable() {
        TankBattleDelegates.OnCheckIfBattleIsOver -= CheckIfBattleOver;
        if (NetworkManager.Singleton != null) {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        TankDelegates.GetHosteeId = null;
    }

    private void OnClientDisconnected(ulong disconnectedId) {
        Debug.Log($"Client {disconnectedId} disconnected.");
        AbruptWin();
    }

    private void CheckIfBattleOver() {
        if (BattleStatus.Value != BattleResult.IN_PROGRESS) return; // Prevent multiple calls if battle is already over
        // If battle over, trigger battle end
        ulong? winnerId = GetWinningClientId();
        if (winnerId != null) {
            EndBattle(winnerId);
        }
    }

    public ulong GetHosteeId() {
        foreach (var kv in NetworkManager.Singleton.ConnectedClients) {
            if (kv.Key != 0) {
                return kv.Key;
            }
        }
        throw new KeyNotFoundException("Could not find hostee id!");
    }
    
    private void AbruptWin() {
        Time.timeScale = 0;
        if (ArenaUIManager.Instance == null) return;
        ArenaUIManager.Instance.ShowWinScreen(true);
        postProcessingVolume.profile = winVolumeProfile;
        ArenaUIManager.Instance.HideBattleUI();
        StartCoroutine(StartShutDownGame(true));
    }

    private IEnumerator StartShutDownGame(bool abrupt = false) {
        yield return new WaitForSecondsRealtime(4f);
        // Scene networkedScene = SceneManager.GetSceneByName("ArenaScene");
        // foreach (GameObject go in networkedScene.GetRootGameObjects())
        // {
        //     Debug.Log("in outer foreach");
        //     NetworkObject[] netObjects = go.GetComponentsInChildren<NetworkObject>(true);
        //     foreach (NetworkObject netObj in netObjects)
        //     {
        //         Debug.Log("in inner foreach");
        //         if (netObj.IsSpawned && NetworkManager.Singleton.IsServer)
        //         {
        //             netObj.Despawn(true);
        //         }
        //     }
        // }
        yield return new WaitForSecondsRealtime(1f);
        if (IsServer) {
            SetTimeScaleClientRpc(1f);
            EndGameClientRpc();
        }
        NetworkManager.Singleton.Shutdown();
        if (abrupt) {
            SceneManager.LoadScene("BootstrapScene");
            LocalSceneManager.Instance.LoadTitleScene();
        }
        
    }
 

    
    
    

    [ClientRpc]
    private void EndGameClientRpc()
    {
        SceneManager.LoadScene("BootstrapScene");
        LocalSceneManager.Instance.LoadTitleScene();
        Debug.Log("Game ended for client.");
        // LanDiscovery.Instance.StartListening();
        
        
        
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
        if (_endBattleCutscene != null) return;
        _endBattleCutscene = StartCoroutine(EndBattleCutscene(winningClientId));
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

    [ClientRpc]
    private void SetTimeScaleClientRpc(float timeScale) {
        Time.timeScale = timeScale;
    }
    
    private IEnumerator EndBattleCutscene(ulong? winningClientId) {
        if (winningClientId == null) {
            throw new Exception("WinningClient is null!");
        }
        
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

        if (IsServer) {
            SetTimeScaleClientRpc(0.2f);
        }
        
        // Disintegrate all existing projectiles in the airfield
        if (IsServer) {
            while (airfieldTransform.childCount > 0) {
                airfieldTransform.GetChild(0).GetComponent<AmmoCollision>().WreckAmmo(true);
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }
        
        // Disable battle UI (e.g. ammo slot destinations, and ammo shop)
        HideBattleUIClientRpc();
        
        ulong killedTank = winningClientId.Value == 0 ? (ulong) TankDelegates.GetHosteeId?.Invoke()! : 0;
        // Focus camera onto killed tank
        if (killedTank == 0) {
            FocusHostCameraClientRpc();
        } else {
            FocusHosteeCameraClientRpc();
        } 
        
        StartCoroutine(PlayDeathAnim(killedTank));
        
        
        yield return new WaitForSecondsRealtime(10f);
        if (IsServer) {
            SetTimeScaleClientRpc(0f);
        }
        StartCoroutine(StartShutDownGame());
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
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/SFX/Explosion"), Random.Range(0.8f, 1.2f));
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
