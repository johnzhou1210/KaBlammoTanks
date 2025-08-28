using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class TanksManager : MonoBehaviour {
    private TankController hostTankController, hosteeTankController;
    [SerializeField] private GameObject hostTankGO, hosteeTankGO;
    void OnEnable() {
        NetworkObject host = NetworkManager.Singleton.ConnectedClients[0].PlayerObject;
        NetworkObject hostee = NetworkManager.Singleton.ConnectedClients[1].PlayerObject;
        hostTankController = host.GetComponent<TankController>();
        hosteeTankController = hostee.GetComponent<TankController>();
        
        TankDelegates.GetHostTankController = () => hostTankController;
        TankDelegates.GetHosteeTankController = () => hosteeTankController;
        TankDelegates.GetHostTankGameObject = () => hostTankGO;
        TankDelegates.GetHosteeTankGameObject = () => hosteeTankGO;
        
        hostTankController.enabled = true;
        hosteeTankController.enabled = true;

    }

    void OnDisable() {
        TankDelegates.GetHostTankController = null;
        TankDelegates.GetHosteeTankController = null;
        TankDelegates.GetHostTankGameObject = null;
        TankDelegates.GetHosteeTankGameObject = null;
    }

}
