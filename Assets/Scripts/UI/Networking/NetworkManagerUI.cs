using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour {
    [SerializeField] private Button serverButton, hostButton, clientButton;

    private void Awake() {
        serverButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartServer();
        });
        hostButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
        });
        clientButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
        });
    }

}
