using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class TankController : MonoBehaviour {
    private const float UpperCannonHeight = 3.5f;
    [field: SerializeField] public GameObject Barrel { get; private set; }
    [field: SerializeField] public int TankMaxHealth { get; private set; } = 100;
    [field: SerializeField] public int TankId { get; private set; }
    [SerializeField] bool EnemyAI = false;
    public int TankHealth { get; private set; }
    void Start() {
        TankHealth = TankMaxHealth;
        if (EnemyAI) {
            StartCoroutine(EnemyAICoroutine());
        }
        TankDelegates.InvokeOnUpdateTankHealthUI(TankId, TankHealth);
    }
    void OnEnable() {
        TankDelegates.OnProjectileFire += FireProjectile;
        TankDelegates.OnTakeDamage += TakeDamage;
    }
    void OnDisable() {
        TankDelegates.OnProjectileFire -= FireProjectile;
        TankDelegates.OnTakeDamage -= TakeDamage;
    }

    private IEnumerator EnemyAICoroutine() {
        while (TankHealth > 0) {
            yield return new WaitForSeconds(Random.Range(0.25f, 6f));
            AmmoData[] allProjectileData = Resources.LoadAll<AmmoData>("ScriptableObjects/Projectiles"); 
            FireProjectile(allProjectileData[Random.Range(0, allProjectileData.Length)], Random.Range(0,2) != 0, 1);
        }
        yield return null;
    }
    
    public void FireProjectile(AmmoData ammoData, bool isUpperCannon, int playerId) {
        if (TankId != playerId) return;
        Vector3 startPos = Barrel.transform.position;
        Vector3 endPos = (TankDelegates.GetTankControllerById?.Invoke(TankId == 0 ? 1 : 0 )!).Barrel.transform.position;
        StartCoroutine(FireCoroutine(ammoData, startPos, endPos, isUpperCannon));
        Instantiate(Resources.Load<GameObject>("Prefabs/VFX/SmokeEffect"), startPos, Quaternion.identity);
    }
    private IEnumerator FireCoroutine(AmmoData projectileData, Vector3 startPos, Vector3 endPos, bool archedTrajectory) {
        GameObject projectile = Instantiate(projectileData.ProjectilePrefab, startPos, quaternion.identity);
        BezierProjectile moveScript = projectile.GetComponent<BezierProjectile>();
        AmmoCollision collisionChecker = projectile.GetComponent<AmmoCollision>();
        collisionChecker.ProjectileData = projectileData;
        collisionChecker.OwnerId = TankId;
        moveScript.Launch(startPos, endPos, archedTrajectory ? UpperCannonHeight : 0f, 10f / projectileData.Speed);
        if (TankId == 1) moveScript.FlipX();
        if (projectile.TryGetComponent<AmmoRotate>(out AmmoRotate rotateScript)) {
            rotateScript.ReverseRotation();
        }
        yield return null;
    }
    private void TakeDamage(int playerId, int damage) {
        if (TankId != playerId) return;
        TankHealth = Math.Clamp(TankHealth - damage, 0, TankMaxHealth);
        TankDelegates.InvokeOnUpdateTankHealthUI(playerId, TankHealth);
    }
}
