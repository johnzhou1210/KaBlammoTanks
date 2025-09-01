using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class LanDiscovery : MonoBehaviour
{
    public static LanDiscovery Instance;

    [Header("Discovery Settings")]
    [SerializeField] private int broadcastPort = 47777; // Shared discovery port
    [SerializeField] private float broadcastInterval = 1f;
    [SerializeField] private float hostTimeout = 5f;

    private UdpClient _udpClient;
    private Thread _listenThread;
    private bool _running;
    private UdpClient _broadcastSender;

    private readonly Dictionary<string, (int port, double lastSeen)> _activeHosts = new();

    private Coroutine _broadcastCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDisable()
    {
        Debug.LogWarning("Stopped listening via LanDiscovery OnDisable");
        StopListening();
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost) {
            if (_broadcastCoroutine != null) { // This is a way to check if host is broadcasting
                Debug.LogWarning("StopBroadcasting called on LanDiscovery OnDisable");
                StopBroadcasting();
            }
        }
        
    }

    private void Update()
    {
        // Remove stale hosts
        double now = (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
        List<string> toRemove = new();
        lock (_activeHosts)
        {
            foreach (var kvp in _activeHosts)
            {
                if (now - kvp.Value.lastSeen > hostTimeout)
                    toRemove.Add(kvp.Key);
            }
            foreach (var key in toRemove)
                _activeHosts.Remove(key);
        }
    }
    


    public void Disconnect() {
        NetworkManager.Singleton.Shutdown();
    }
    

    #region Hosting

    public void StartBroadcasting(int gamePort)
    {
        _broadcastCoroutine = StartCoroutine(BroadcastLoop(gamePort));
    }

    public void StopBroadcasting()
    {
        if (_broadcastCoroutine != null && NetworkManager.Singleton != null) {
            StopCoroutine(_broadcastCoroutine);
            _broadcastCoroutine = null;
        }
        _broadcastSender?.Close();
        Debug.Log("Setting _broadcastSender to null");
        _broadcastSender = null;
        Disconnect();
        Debug.Log($"Stopped broadcasting to port {broadcastPort}");
        Debug.LogWarning("STOPPED BROADCASTING!");
        
    }

    private IEnumerator BroadcastLoop(int gamePort)
    {
        _broadcastSender?.Close();
        _broadcastSender = new UdpClient();
        _broadcastSender.EnableBroadcast = true;

        string broadcastIP = GetBroadcastAddress();
        IPEndPoint endpoint = new(IPAddress.Parse(broadcastIP), broadcastPort);

        // Send multiple initial broadcasts immediately for reliability
        for (int i = 0; i < 3; i++)
        {
            byte[] data = Encoding.UTF8.GetBytes($"GAME:{gamePort}");
            _broadcastSender.Send(data, data.Length, endpoint);
            yield return new WaitForSeconds(0.1f);
        }

        while (true) {
            if (_broadcastSender == null) yield break;
            byte[] data = Encoding.UTF8.GetBytes($"GAME:{gamePort}");
            _broadcastSender.Send(data, data.Length, endpoint);
            yield return new WaitForSeconds(broadcastInterval);
        }
    }

    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        }
        return "127.0.0.1";
    }

    private string GetBroadcastAddress()
    {
        string localIP = GetLocalIPAddress();
        string[] parts = localIP.Split('.');
        return $"{parts[0]}.{parts[1]}.{parts[2]}.255";
        
    }

    #endregion

    #region Listening
    
    public void StartListening()
    {
        if (_udpClient != null) return;

        try
        {
            _udpClient = new UdpClient(broadcastPort)
            {
                EnableBroadcast = true
            };
            _running = true;

            _listenThread = new Thread(ListenLoop) { IsBackground = true };
            _listenThread.Start();
        }
        catch (SocketException e)
        {
            Debug.LogWarning($"LanDiscovery could not bind to port {broadcastPort}: {e.Message}");
        }
    }

    public void StopListening()
    {
        _running = false;
        if (_udpClient != null)
        {
            try { _udpClient.Close(); } catch { }
            _udpClient = null;
        }
        if (_listenThread != null && _listenThread.IsAlive)
        {
            _listenThread.Join(); // Wait for thread to terminate
            _listenThread = null;
        }
  
        Debug.Log($"Stopped listening to port {broadcastPort}");
    }

    private void ListenLoop()
    {
        IPEndPoint remoteEP = new(IPAddress.Any, broadcastPort);

        try
        {
            while (_running)
            {
                byte[] data = _udpClient.Receive(ref remoteEP);
                string msg = Encoding.UTF8.GetString(data);
                Debug.Log($"Received packet from {remoteEP.Address}: {msg}");

                
                
                if (msg.StartsWith("GAME:"))
                {
                    int port = int.Parse(msg.Substring(5));
                    string hostIP = remoteEP.Address.ToString();

                    double now = (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
                    lock (_activeHosts)
                    {
                        _activeHosts[hostIP] = (port, now);
                    }
                }
            }
        }
        catch (SocketException)
        {
            // Socket closed while waiting, expected on shutdown
        }
    }

    #endregion

    #region Query

    public List<(string ip, int port)> GetActiveHosts()
    {
        lock (_activeHosts)
        {
            var result = new List<(string, int)>();
            foreach (var kvp in _activeHosts)
            {
                result.Add((kvp.Key, kvp.Value.port));
            }
            return result;
        }
    }

    #endregion
}
