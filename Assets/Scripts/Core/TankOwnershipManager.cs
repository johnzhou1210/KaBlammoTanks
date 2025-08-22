using Unity.Netcode;
using UnityEngine;

public class TankOwnershipManager : NetworkBehaviour {
    
    
    public override void OnNetworkSpawn() {
        if (IsOwner) {
            Debug.Log("This is local player's tank!");
        } else {
            Debug.Log("This tank belongs to another player: " + OwnerClientId);
        }
    }
}
