using System;
using System.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class TankController : NetworkBehaviour {
    private const float UpperCannonHeight = 5.2f;
    public GameObject Tank { get; private set; }
    public NetworkVariable<int> TankMaxHealth = new NetworkVariable<int>();
    [SerializeField] private bool EnemyAI;
    public NetworkVariable<float> TimeAlive = new NetworkVariable<float>();
    public NetworkVariable<int> TankHealth = new NetworkVariable<int>();
    private AmmoDatabase _ammoDatabase;

    private Coroutine _aiCoroutine; // Only used if AI is controlling tank
    
    private void Update() {
        if (NetworkManager.Singleton == null)
            return;
        if (IsServer) {
            TimeAlive.Value += NetworkManager.Singleton.NetworkTickSystem.TickRate;
        }
    }
    private void Start() {
        _ammoDatabase = Resources.Load<AmmoDatabase>("ScriptableObjects/AmmoDatabase");
    }
    private void OnEnable() {
        Debug.Log($"TankController enabled on {(IsServer ? "SERVER" : "CLIENT")} with OwnerId={OwnerClientId}");
        if (!EnemyAI) {
            TankDelegates.OnTakeDamage += TakeDamage;
        }
        TankBattleDelegates.OnInitTanks += InitTank;
        if (OwnerClientId == 0) {
            Tank = GameObject.FindGameObjectWithTag("HostTank");
        } else {
            Tank = GameObject.FindGameObjectWithTag("HosteeTank");
        }
        if (IsServer) {
            Debug.Log("Initialized Tank Health Values");
            TankMaxHealth.Value = 100;
            TankHealth.Value = TankMaxHealth.Value;
        }
        StartCoroutine(InitialHealthUIUpdateWhenReady());
    }
    private IEnumerator InitialHealthUIUpdateWhenReady() {
        yield return new WaitUntil((() => TankMaxHealth.Value > 0));
        TankDelegates.InvokeOnUpdateTankHealthUI(EnemyAI ? 1 : OwnerClientId, TankHealth.Value, TankMaxHealth.Value);
    }
    private void OnDisable() {
        if (!EnemyAI) {
            TankDelegates.OnTakeDamage -= TakeDamage;
        }
        TankBattleDelegates.OnInitTanks -= InitTank;
    }
    private IEnumerator EnemyAICoroutine() {
        Debug.LogWarning("STARTED ENEMY AI COROUTINE");
        while (TankHealth.Value > 0) {
            yield return new WaitForSeconds(Random.Range(0.125f, 3f));
            if (TankHealth.Value <= 0) yield break;
            // Look up ammo from database
            AmmoData ammoData = _ammoDatabase.GetRandomAmmoWeighted();
            FireProjectileServerRpc(AmmoDatabase.GetAmmoRequest(ammoData), Random.Range(0, 2) != 0);
        }
        yield return null;
    }
    [ServerRpc(RequireOwnership = false)]
    public void FireProjectileServerRpc(AmmoRequest request, bool isUpperCannon, ServerRpcParams serverRpcParams = default) {
        ulong senderClientId = EnemyAI ? 1 : serverRpcParams.Receive.SenderClientId;
        Debug.Log($"Client {senderClientId} requested to fire ammo: {request.AmmoName}");
        if (EnemyAI) {
            EnemyAIFireProjectile(request, isUpperCannon);
        } else if (NetworkManager.Singleton.ConnectedClients.TryGetValue(senderClientId, out var client)) {
            RegularPlayerFireProjectile(client, request, senderClientId, isUpperCannon);
        }
    }
    private void EnemyAIFireProjectile(AmmoRequest request, bool isUpperCannon) {
        FireProjectile(request, 1, isUpperCannon);
    }
    private void RegularPlayerFireProjectile(NetworkClient client, AmmoRequest request, ulong senderClientId, bool isUpperCannon) {
        NetworkObject clientObject = client.PlayerObject;
        if (clientObject != null) {
            FireProjectile(request, senderClientId, isUpperCannon);
        }
    }
    private void FireProjectile(AmmoRequest request, ulong senderClientId, bool isUpperCannon) {
        // Look up ammo from database
        AmmoData ammoData = _ammoDatabase.GetAmmo(request.AmmoName);
        if (ammoData == null) {
            Debug.LogWarning("Invalid ammo name: " + request.AmmoName);
            return;
        }

        // Get appropriate barrel position
        GameObject shooterTankGameObject = senderClientId == 0 ? TankDelegates.GetHostTankGameObject?.Invoke() : TankDelegates.GetHosteeTankGameObject?.Invoke();
        GameObject targetTankGameObject = senderClientId == 0 ? TankDelegates.GetHosteeTankGameObject?.Invoke() : TankDelegates.GetHostTankGameObject?.Invoke();
        Vector3? shootingBarrelPosition = shooterTankGameObject!.transform.Find("Barrel").transform.position;
        Vector3? targetBarrelPosition = targetTankGameObject!.transform.Find("Barrel").transform.position;
        Vector3 startPos = (Vector3)shootingBarrelPosition;
        Vector3 endPos = (Vector3)targetBarrelPosition;
        StartCoroutine(FireCoroutine(senderClientId, ammoData, startPos, endPos, isUpperCannon));
        GameObject smokeEffect = Instantiate(Resources.Load<GameObject>("Prefabs/VFX/SmokeEffect"), startPos - Vector3.forward, Quaternion.identity);
        NetworkObject smokeNetworkObject = smokeEffect.GetComponent<NetworkObject>();
        smokeNetworkObject.Spawn();
        FireCannonSoundClientRpc();
    }
    [ClientRpc]
    private void FireCannonSoundClientRpc() {
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/SFX/CannonFire"), Random.Range(0.8f, 1.2f));
    }
    private IEnumerator FireCoroutine(ulong senderClientId, AmmoData projectileData, Vector3 startPos, Vector3 endPos, bool archedTrajectory) {
        var projectile = Instantiate(projectileData.ProjectilePrefab, startPos, Quaternion.identity);
        NetworkObject projectileNetworkObject = projectile.GetComponent<NetworkObject>();
        projectileNetworkObject.Spawn();
        projectile.transform.parent = AirFieldDelegates.GetAirFieldTransform?.Invoke();
        var moveScript = projectile.GetComponent<BezierProjectile>();
        var collisionChecker = projectile.GetComponent<AmmoCollision>();
        Debug.Log($"Attempting to initialize projectile with senderClientId: {senderClientId} and projectileData: {projectileData}");
        collisionChecker.Initialize(senderClientId, projectileData);
        moveScript.Launch(startPos, endPos, archedTrajectory ? UpperCannonHeight : 0f, 10f / projectileData.Speed, TimeAlive.Value);
        if (senderClientId == TankDelegates.GetHosteeId?.Invoke())
            moveScript.FlipXClientRpc();
        if (projectile.TryGetComponent(out AmmoRotate rotateScript))
            rotateScript.ReverseRotation();
        yield return null;
    }
    private void TakeDamage(int damage, ulong targetId) {
        if (GameSessionManager.Instance.GameSessionType == GameSessionType.MULTIPLAYER) {
            // Client should never be running this method
            if (targetId == OwnerClientId) {
                Debug.Log($"Client {OwnerClientId} taking {damage} damage");
                TankHealth.Value = Math.Clamp(TankHealth.Value - damage, 0, TankMaxHealth.Value);
            }
            UpdateAllTankHealthUIClientRpc(TankHealth.Value, TankMaxHealth.Value);
        } else {
            // Singleplayer logic
            TankController targetTankController = (targetId == OwnerClientId) ? this : TankDelegates.GetHosteeTankController?.Invoke();
            if (targetTankController == null) {
                throw new Exception("targetTankController is null");
            }
            Debug.Log($"Target {targetId} taking {damage} damage {EnemyAI}");
            targetTankController.TankHealth.Value = Math.Clamp(targetTankController.TankHealth.Value - damage, 0, targetTankController.TankMaxHealth.Value);
            if (!EnemyAI) {
                targetTankController.UpdateAllTankHealthUIClientRpc(targetTankController.TankHealth.Value, targetTankController.TankMaxHealth.Value);
            } else {
                if (targetTankController.TankHealth.Value <= 0) {
                    if (_aiCoroutine != null) {
                        StopCoroutine(_aiCoroutine);
                        _aiCoroutine = null;
                    }
                }
            }
        }
        // Check for game end condition
        TankBattleDelegates.InvokeOnCheckIfBattleIsOver();
    }
    [ClientRpc]
    private void UpdateAllTankHealthUIClientRpc(int newHealth, int newMaxHealth) {
        Debug.Log($"UpdateAllTankHealthUIClientRpc {newHealth} | {newMaxHealth}");
        TankDelegates.InvokeOnUpdateTankHealthUI(EnemyAI ? 1 : OwnerClientId, newHealth, newMaxHealth);
    }
    private void InitTank() {
        if (EnemyAI)
            _aiCoroutine = StartCoroutine(EnemyAICoroutine());
    }
    public override string ToString() {
        return $"Tank {OwnerClientId}: health: {TankHealth.Value}, maxHealth: {TankMaxHealth.Value}";
    }
}
