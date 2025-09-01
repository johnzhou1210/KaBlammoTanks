using System;
using System.Collections;
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
        StartCoroutine(RestartHostRoutine());
    }

    private IEnumerator RestartHostRoutine() {
        // Ensure previous session is cleaned up
        LanDiscovery.Instance.Disconnect();

        yield return null;
        yield return new WaitForSeconds(0.1f);

        _hostPort = PortAllocator.GetNextAvailablePort();
        _transport.SetConnectionData("0.0.0.0", (ushort)_hostPort);

        Debug.Log($"Host starting on port {_hostPort}");
        NetworkManager.Singleton.StartHost();
        Debug.Log("Host StartHost completed");

        Debug.LogWarning("StartBroadcasting via LocalHostManager RestartHostRoutine");
        LanDiscovery.Instance.StartBroadcasting(_hostPort);
        
    }


    public void StartClientSession(string hostIP, int hostPort) {
        StartCoroutine(RestartClientRoutine(hostIP, hostPort));
    }

    private IEnumerator RestartClientRoutine(string hostIP, int hostPort) {
        LanDiscovery.Instance.Disconnect();
        
    
        _transport.SetConnectionData(hostIP, (ushort)hostPort);

        yield return null;

        Debug.Log($"Client connecting to {hostIP}:{hostPort}");
        bool started = NetworkManager.Singleton.StartClient();
        Debug.Log($"Client StartClient returned {started}");
      
    }

    private void OnApplicationQuit() {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost) {
            Debug.LogWarning("Stopped broadcasting via LocalHostManager OnApplicationQuit");
            LanDiscovery.Instance.StopBroadcasting();
            NetworkManager.Singleton.Shutdown();
        }
    }

}
