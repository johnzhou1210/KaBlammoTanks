using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class PingDisplay : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI pingText;

    void Update() {
//         pingText.SetText("");
//
// // Make sure NetworkManager exists and is running
//         if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
//             return;
//
//         var transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;
//         if (transport == null)
//             return;
//
// // Only show RTT if we're a client (not host/server-only) AND connected
//         if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost && NetworkManager.Singleton.IsConnectedClient)
//         {
//             var clientId = NetworkManager.Singleton.LocalClientId;
//             ulong rtt = transport.GetCurrentRtt(clientId); // returns -1 if no RTT available
//
//             if (rtt >= 0)
//                 pingText.SetText($"{rtt} ms");
//         }
//         else if (NetworkManager.Singleton.IsHost)
//         {
//             // Optional: show "Host" instead of ping
//             pingText.SetText("0 ms (Host)");
//         }

    }
}
