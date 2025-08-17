using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class PlayerNetwork : NetworkBehaviour {
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction buttonAction;

    private void OnEnable() {
        moveAction.Enable();
        buttonAction.Enable();
    }

    private void OnDisable() {
        moveAction.Disable();
        buttonAction.Disable();
    }

    public override void OnNetworkSpawn() {
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) => {
            Debug.Log(OwnerClientId + "; " + newValue._int + "; " + newValue._bool + "; " + newValue.message);
        };
    }

    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(new MyCustomData {
        _int = 56,
        _bool = true
    }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public struct MyCustomData : INetworkSerializable {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }
    
    private void Update() {
        
        if (!IsOwner) return;

        if (buttonAction.triggered) {
            randomNumber.Value = new MyCustomData {
                _int = Random.Range(0, 100),
                _bool = Random.Range(0,2) == 0,
                message = new FixedString128Bytes("Hello World!")
            };
        }
        
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 movement = new Vector3(input.x, input.y, 0f) * (1f * Time.deltaTime);
        transform.Translate(movement);
    }
}
