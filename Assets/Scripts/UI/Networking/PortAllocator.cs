using System.Net.Sockets;
using UnityEngine;

public static class PortAllocator {
   private static ushort _nextPort = 7777;
   
   public static ushort GetNextAvailablePort() {
      while (true) {
         ushort candidate = _nextPort++;
         if (IsPortFree(candidate))
            return candidate;
      }
   }

   private static bool IsPortFree(ushort port) {
      try {
         var listener = new UdpClient(port);
         listener.Close();
         return true;
      } catch (SocketException) {
         return false;
      }
   }
   
}
