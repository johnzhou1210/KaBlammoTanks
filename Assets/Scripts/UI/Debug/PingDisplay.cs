using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class PingDisplay : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI pingText;

    void Update() {
        pingText.SetText("");
        if (NetworkManager.Singleton == null) return;
        if (NetworkManager.Singleton.NetworkConfig == null) return;
        var transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;
        if (transport != null && NetworkManager.Singleton.IsClient) {
            ulong rtt = transport.GetCurrentRtt(NetworkManager.Singleton.LocalClientId);
            pingText.SetText($"{rtt} ms");
        }
    }
}
