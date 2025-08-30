using System;
using System.Net.Sockets;

public static class PortAllocator {
    private static readonly ushort StartingPort = 7777;

    public static ushort GetNextAvailablePort() {
        for (ushort port = StartingPort; port < 8000; port++) {
            if (IsPortFree(port))
                return port;
        }
        throw new Exception("No free port found in range");
    }

    private static bool IsPortFree(ushort port) {
        try {
            using var udp = new UdpClient(port);
            return true;
        } catch (SocketException) {
            return false;
        }
    }
}
