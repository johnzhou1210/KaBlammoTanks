using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNetwork : NetworkBehaviour {
    [SerializeField] private InputAction moveAction;

    private void OnEnable() {
        moveAction.Enable();
    }

    private void OnDisable() {
        moveAction.Disable();
    }

    private void Update() {
        if (!IsOwner) return;
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 movement = new Vector3(input.x, input.y, 0f) * (1f * Time.deltaTime);
        transform.Translate(movement);
    }
}
