using System;
using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour {
    public float moveSpeed = 5f;
    private void Update() {
        if (!isLocalPlayer)
            return;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        transform.Translate(new Vector3(h, 0, v) * (moveSpeed * Time.deltaTime));
    }
    private void OnGUI() {
        if (!NetworkClient.isConnected && !NetworkServer.active) {
            if (GUI.Button(new Rect(10, 10, 150, 30), "Start Host"))
                NetworkManager.singleton.StartHost();
            if (GUI.Button(new Rect(10, 50, 150, 30), "Start Client")) {
                NetworkManager.singleton.networkAddress = "192.168.x.x"; // Host's local IP
                NetworkManager.singleton.StartClient();
            }
        }
    }
}
