using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

    private void OnEnable()
    {
        StartListening();
    }

    private void OnDisable()
    {
        StopListening();
        StopBroadcasting();
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

    #region Hosting

    public void StartBroadcasting(int gamePort)
    {
        StopBroadcasting();
        StartCoroutine(BroadcastLoop(gamePort));
    }

    public void StopBroadcasting()
    {
        StopAllCoroutines();
        _broadcastSender?.Close();
        _broadcastSender = null;
        Debug.Log($"Stopped broadcasting to port {broadcastPort}");
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

        while (true)
        {
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

    private void StartListening()
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

    private void StopListening()
    {
        _running = false;
        _udpClient?.Close();
        _udpClient = null;
        _listenThread = null;
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
