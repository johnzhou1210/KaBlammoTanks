using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class PlayerNetwork : NetworkBehaviour {
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction createAction, deleteAction;

    [SerializeField] private Transform spawnedObjectPrefab;
    private Transform spawnedObjectTransform;
    
    private void OnEnable() {
        moveAction.Enable();
        createAction.Enable();
        deleteAction.Enable();
    }

    private void OnDisable() {
        moveAction.Disable();
        createAction.Disable();
        deleteAction.Disable();
    }

    public override void OnNetworkSpawn() {
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) => {
            Debug.Log(OwnerClientId + "; " + newValue._int + "; " + newValue._bool + "; " + newValue.message);
        };
        StartCoroutine(WaitForTankInit());
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

        if (createAction.triggered) {
            spawnedObjectTransform = Instantiate(spawnedObjectPrefab);
            spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);

            // TestServerRpc(new ServerRpcParams());
            // TestClientRpc(new ClientRpcParams{Send = new ClientRpcSendParams { TargetClientIds = new List<ulong>{1}}} );
            // randomNumber.Value = new MyCustomData {
            //     _int = Random.Range(0, 100),
            //     _bool = Random.Range(0,2) == 0,
            //     message = new FixedString128Bytes("Hello World!")
            // };
        }

        if (deleteAction.triggered) {
            Destroy(spawnedObjectTransform.gameObject);
        }
        
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 movement = new Vector3(input.x, input.y, 0f) * (1f * Time.deltaTime);
        transform.Translate(movement);
    }


    [ServerRpc]
    private void TestServerRpc(ServerRpcParams serverRpcParams) {
        Debug.Log("TestServerRpc" + OwnerClientId + ": " + serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void TestClientRpc(ClientRpcParams clientRpcParams) {
        Debug.Log("Testing client rpc");
    }

    private IEnumerator WaitForTankInit() {
        yield return new WaitUntil(() => {
            List<Scene> syncedScenes = NetworkManager.Singleton.SceneManager.GetSynchronizedScenes();
            if (syncedScenes.Contains(SceneManager.GetSceneByName("ArenaScene"))) {
                return true;
            }
            return false;
        });
        // Arena scene is loaded
        TanksManager tanksManager = GameObject.FindWithTag("TanksManager").GetComponent<TanksManager>();
        tanksManager.enabled = true;
    }
    
}
