using System;
using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour {
    public override void OnStartLocalPlayer()
    {
        Camera.main.transform.SetParent(transform);
        Camera.main.transform.localPosition = new Vector3(0, 0, 0);
    }

    void Update()
    {
        if (!isLocalPlayer) { return; }

        Debug.Log("I am the local player!");
    }
}
