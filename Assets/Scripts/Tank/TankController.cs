using System;
using System.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class TankController : NetworkBehaviour {
    private const float UpperCannonHeight = 5.2f;
    [field: SerializeField] public GameObject Barrel { get; private set; }
    [field: SerializeField] public int TankMaxHealth { get; private set; } = 100;
    [SerializeField] private bool EnemyAI;
    public int TankHealth { get; private set; }

    private void Start() {
        TankHealth = TankMaxHealth;
        TankDelegates.InvokeOnUpdateTankHealthUI(OwnerClientId, TankHealth);
    }

    private void OnEnable() {
        // TankDelegates.OnProjectileFire += FireProjectileServerRpc;
        // TankDelegates.OnTakeDamage += TakeDamageServerRpc;
        TankBattleDelegates.OnInitTanks += InitTank;
    }

    private void OnDisable() {
        // TankDelegates.OnProjectileFire -= FireProjectileServerRpc;
        // TankDelegates.OnTakeDamage -= TakeDamageServerRpc;
        TankBattleDelegates.OnInitTanks -= InitTank;
    }

    private IEnumerator EnemyAICoroutine() {
        while (TankHealth > 0) {
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
                AmmoData ammoData = AmmoDatabase.Instance.GetAmmo(request.AmmoName);
                if (ammoData == null) {
                    Debug.LogWarning("Invalid ammo name: " + request.AmmoName);
                    return;
                }
                
                // Get appropriate barrel position
                Vector3? transformPosition = TankDelegates.GetTankControllerById?.Invoke(senderClientId).Barrel.transform.position;
                if (transformPosition != null) {
                    Vector3 startPos = (Vector3)transformPosition;
                    Vector3? position = TankDelegates.GetTankControllerById?.Invoke((ulong)(senderClientId == 0 ? 1 : 0)).Barrel.transform.position;
                    if (position != null) {
                        Vector3 endPos = (Vector3)position;
                        StartCoroutine(FireCoroutine(senderClientId, ammoData, startPos, endPos, isUpperCannon));
                    }
                    Instantiate(Resources.Load<GameObject>("Prefabs/VFX/SmokeEffect"), startPos, Quaternion.identity);
                }
                AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/SFX/CannonFire"), Random.Range(0.8f, 1.2f));


            }
        }
        
        
    }

    private IEnumerator FireCoroutine(ulong senderClientId, AmmoData projectileData, Vector3 startPos, Vector3 endPos,
        bool archedTrajectory) {
        var projectile = Instantiate(projectileData.ProjectilePrefab, startPos, Quaternion.identity);
        projectile.transform.parent = AirFieldDelegates.GetAirFieldTransform?.Invoke();
        var moveScript = projectile.GetComponent<BezierProjectile>();
        var collisionChecker = projectile.GetComponent<AmmoCollision>();
        collisionChecker.Initialize(senderClientId, projectileData);
        moveScript.Launch(startPos, endPos, archedTrajectory ? UpperCannonHeight : 0f, 10f / projectileData.Speed);
        if (senderClientId == 1) moveScript.FlipX();
        if (projectile.TryGetComponent(out AmmoRotate rotateScript)) rotateScript.ReverseRotation();
        yield return null;
    }

    [ClientRpc(RequireOwnership = false)]
    private void TakeDamageClientRpc(ulong targetClientId, int damage, ClientRpcParams clientRpcParams = default) {
        Debug.Log($"Client {targetClientId} taking {damage} damage");
        
        TankHealth = Math.Clamp(TankHealth - damage, 0, TankMaxHealth);
        TankDelegates.InvokeOnUpdateTankHealthUI(targetClientId, TankHealth);
        // Check for game end condition
        TankBattleDelegates.InvokeOnCheckIfBattleIsOver();
    }

    private void InitTank() {
        // if (EnemyAI) StartCoroutine(EnemyAICoroutine());
    }
}