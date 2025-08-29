using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class LocalHostManager : MonoBehaviour {
    private UnityTransport _transport;

    private int _hostPort;

    private void Awake() {
        _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    }

    public void StartHostSession() {
        _hostPort = PortAllocator.GetNextAvailablePort();
        
        _transport.SetConnectionData("0.0.0.0", (ushort)_hostPort);
        NetworkManager.Singleton.StartHost();
        
        LanDiscovery.Instance.StartBroadcasting(_hostPort);
        Debug.Log($"Started host on port {_hostPort}");
    }

    public void StartClientSession(string hostIP, int hostPort) {
        _transport.SetConnectionData(hostIP, (ushort)hostPort);
        NetworkManager.Singleton.StartClient();
        Debug.Log($"Connecting to host {hostIP}:{hostPort}");
    }

    private void OnApplicationQuit() {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost) {
            LanDiscovery.Instance.StopBroadcasting();
        }
    }

}
