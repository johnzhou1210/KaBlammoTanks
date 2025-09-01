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
    void OnEnable() {
        NetworkObject host = NetworkManager.Singleton.ConnectedClients[0].PlayerObject;
        NetworkObject hostee = NetworkManager.Singleton.ConnectedClients[(ulong)TankDelegates.GetHosteeId?.Invoke()!].PlayerObject;
        hostTankController = host.GetComponent<TankController>();
        hosteeTankController = hostee.GetComponent<TankController>();
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
