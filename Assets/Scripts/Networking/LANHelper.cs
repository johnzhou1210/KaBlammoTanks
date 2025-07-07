using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class LANHelper : MonoBehaviour
{
    void Start()
    {
        string localIP = GetLocalIPAddress();
        Debug.Log("Host LAN IP: " + localIP);
    }

    public static string GetLocalIPAddress()
    {
        foreach (var iface in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (iface.AddressFamily == AddressFamily.InterNetwork)
                return iface.ToString();
        }
        return "127.0.0.1";
    }
}
