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
    
    
    public NetworkVariable<int> TankHealth = new NetworkVariable<int>();

    private void OnEnable() {
        Debug.Log($"TankController enabled on {(IsServer ? "SERVER" : "CLIENT")} with OwnerId={OwnerClientId}");
        TankDelegates.OnTakeDamage += TakeDamage;
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
        TankDelegates.InvokeOnUpdateTankHealthUI(OwnerClientId, TankHealth.Value, TankMaxHealth.Value);
    }
    
    private void OnDisable() {
        TankDelegates.OnTakeDamage -= TakeDamage;
        TankBattleDelegates.OnInitTanks -= InitTank;
    }

    private IEnumerator EnemyAICoroutine() {
        while (TankHealth.Value > 0) {
            yield return new WaitForSeconds(Random.Range(0.25f, 6f));
            var allProjectileData = Resources.LoadAll<AmmoData>("ScriptableObjects/Projectiles");
            // FireProjectileServerRpc(allProjectileData[Random.Range(0, allProjectileData.Length)], Random.Range(0, 2) != 0, 1);
        }

        yield return null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void FireProjectileServerRpc(AmmoRequest request, bool isUpperCannon, ServerRpcParams serverRpcParams = default) {
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        Debug.Log($"Client {senderClientId} requested to fire ammo: {request.AmmoName}");

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(senderClientId, out var client)) {
            NetworkObject clientObject = client.PlayerObject;
            if (clientObject != null) {
                // Fire projectile from requesting player's position
                Transform firePoint = clientObject.transform;
                
                // Look up ammo from database
                AmmoDatabase database = Resources.Load<AmmoDatabase>("ScriptableObjects/AmmoDatabase");
                AmmoData ammoData = database.GetAmmo(request.AmmoName);
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
                GameObject smokeEffect =  Instantiate(Resources.Load<GameObject>("Prefabs/VFX/SmokeEffect"), startPos, Quaternion.identity);
                NetworkObject smokeNetworkObject = smokeEffect.GetComponent<NetworkObject>();
                smokeNetworkObject.Spawn();
               
                
                AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/SFX/CannonFire"), Random.Range(0.8f, 1.2f));


            }
        }
        
        
    }

    private IEnumerator FireCoroutine(ulong senderClientId, AmmoData projectileData, Vector3 startPos, Vector3 endPos,
        bool archedTrajectory) {
        var projectile = Instantiate(projectileData.ProjectilePrefab, startPos, Quaternion.identity);
        NetworkObject projectileNetworkObject = projectile.GetComponent<NetworkObject>();
        projectileNetworkObject.Spawn();
        projectile.transform.parent = AirFieldDelegates.GetAirFieldTransform?.Invoke();
        
        
        var moveScript = projectile.GetComponent<BezierProjectile>();
        var collisionChecker = projectile.GetComponent<AmmoCollision>();
        Debug.Log($"Attempting to initialize projectile with senderClientId: {senderClientId} and projectileData: {projectileData}");
        collisionChecker.Initialize(senderClientId, projectileData);
        moveScript.Launch(startPos, endPos, archedTrajectory ? UpperCannonHeight : 0f, 10f / projectileData.Speed);
        if (senderClientId == 1) moveScript.FlipXClientRpc();
        if (projectile.TryGetComponent(out AmmoRotate rotateScript)) rotateScript.ReverseRotation();
        yield return null;
    }

    
    private void TakeDamage(int damage, ulong targetId) {
        // Client should never be running this method
        if (targetId == OwnerClientId) {
            Debug.Log($"Client {OwnerClientId} taking {damage} damage");
            TankHealth.Value = Math.Clamp(TankHealth.Value - damage, 0, TankMaxHealth.Value);
        }
        
        UpdateAllTankHealthUIClientRpc(TankHealth.Value, TankMaxHealth.Value);
        
        // Check for game end condition
        TankBattleDelegates.InvokeOnCheckIfBattleIsOver();
        
    }

    [ClientRpc]
    private void UpdateAllTankHealthUIClientRpc(int newHealth, int newMaxHealth) {
        TankDelegates.InvokeOnUpdateTankHealthUI(OwnerClientId, newHealth, newMaxHealth);
    }

    private void InitTank() {
        // if (EnemyAI) StartCoroutine(EnemyAICoroutine());
    }

    public override string ToString() {
        return $"Tank {OwnerClientId}: health: {TankHealth.Value}, maxHealth: {TankMaxHealth.Value}";
    }
}