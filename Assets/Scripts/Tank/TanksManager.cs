using System;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TanksManager : MonoBehaviour {
    private TankController hostTankController, hosteeTankController;
    [SerializeField] private GameObject hostTankGO, hosteeTankGO;
    [SerializeField] public Color HostColor, HosteeColor;
    [SerializeField] private TankController aITankController;
    
    void OnEnable() {
        Initialize();
    }

    private void Initialize() {

        hostTankController = GetHostTankController();
        hosteeTankController = GetHosteeTankController();

        if (GameSessionManager.Instance.GameSessionType == GameSessionType.SINGLEPLAYER) {
            hosteeTankController.enabled = true;
        }
        
        TankDelegates.GetHostTankController = () => hostTankController;
        TankDelegates.GetHosteeTankController = () => hosteeTankController;
        TankDelegates.GetHostTankGameObject = () => hostTankGO;
        TankDelegates.GetHosteeTankGameObject = () => hosteeTankGO;
        hostTankController.enabled = true;
        hosteeTankController.enabled = true;
        
        if (NetworkManager.Singleton.IsHost) {
            hostTankGO.GetComponent<TankDisplay>().SetIdentityMarker("YOU", HostColor);
            hosteeTankGO.GetComponent<TankDisplay>().SetIdentityMarker("FOE", HosteeColor);
        } else {
            hostTankGO.GetComponent<TankDisplay>().SetIdentityMarker("FOE", HostColor);
            hosteeTankGO.GetComponent<TankDisplay>().SetIdentityMarker("YOU", HosteeColor);
        }
    }

    private NetworkObject GetHost() {
        return NetworkManager.Singleton.ConnectedClients[0].PlayerObject;
    }

    private TankController GetHostTankController() {
        return GetHost().GetComponent<TankController>();
    }

    private TankController GetHosteeTankController() {
        if (GameSessionManager.Instance.GameSessionType == GameSessionType.MULTIPLAYER) {
            return GetHostee().GetComponent<TankController>();
        }
        // Singleplayer logic
        return aITankController;
    }

    private NetworkObject GetHostee() {
        if (GameSessionManager.Instance.GameSessionType == GameSessionType.MULTIPLAYER) {
            return NetworkManager.Singleton.ConnectedClients[(ulong)TankDelegates.GetHosteeId?.Invoke()!].PlayerObject;
        }
        // if singleplayer, just return null
        return null;
    }


    void OnDisable() {
        TankDelegates.GetHostTankController = null;
        TankDelegates.GetHosteeTankController = null;
        TankDelegates.GetHostTankGameObject = null;
        TankDelegates.GetHosteeTankGameObject = null;
    }
    public GameObject GetHostTankGO() {
        return hostTankGO;
    }
    public GameObject GetHosteeTankGO() {
        return hosteeTankGO;
    }
}
