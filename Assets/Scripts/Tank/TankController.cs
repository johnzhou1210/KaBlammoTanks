using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class TankController : MonoBehaviour {
    private const float UpperCannonHeight = 5.2f;
    [field: SerializeField] public GameObject Barrel { get; private set; }
    [field: SerializeField] public int TankMaxHealth { get; private set; } = 100;
    [field: SerializeField] public int TankId { get; private set; }
    [SerializeField] private bool EnemyAI;
    public int TankHealth { get; private set; }

    private void Start() {
        TankHealth = TankMaxHealth;
        TankDelegates.InvokeOnUpdateTankHealthUI(TankId, TankHealth);
    }

    private void OnEnable() {
        TankDelegates.OnProjectileFire += FireProjectile;
        TankDelegates.OnTakeDamage += TakeDamage;
        TankBattleDelegates.OnInitTanks += InitTank;
    }

    private void OnDisable() {
        TankDelegates.OnProjectileFire -= FireProjectile;
        TankDelegates.OnTakeDamage -= TakeDamage;
        TankBattleDelegates.OnInitTanks -= InitTank;
    }

    private IEnumerator EnemyAICoroutine() {
        while (TankHealth > 0) {
            yield return new WaitForSeconds(Random.Range(0.25f, 6f));
            var allProjectileData = Resources.LoadAll<AmmoData>("ScriptableObjects/Projectiles");
            FireProjectile(allProjectileData[Random.Range(0, allProjectileData.Length)], Random.Range(0, 2) != 0, 1);
        }

        yield return null;
    }

    public void FireProjectile(AmmoData ammoData, bool isUpperCannon, int playerId) {
        if (TankId != playerId) return;
        var startPos = Barrel.transform.position;
        var endPos = (TankDelegates.GetTankControllerById?.Invoke(TankId == 0 ? 1 : 0)!).Barrel.transform.position;
        StartCoroutine(FireCoroutine(ammoData, startPos, endPos, isUpperCannon));
        Instantiate(Resources.Load<GameObject>("Prefabs/VFX/SmokeEffect"), startPos, Quaternion.identity);
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/SFX/CannonFire"),
            Random.Range(0.8f, 1.2f));
    }

    private IEnumerator FireCoroutine(AmmoData projectileData, Vector3 startPos, Vector3 endPos,
        bool archedTrajectory) {
        var projectile = Instantiate(projectileData.ProjectilePrefab, startPos, Quaternion.identity);
        projectile.transform.parent = AirFieldDelegates.GetAirFieldTransform?.Invoke();
        var moveScript = projectile.GetComponent<BezierProjectile>();
        var collisionChecker = projectile.GetComponent<AmmoCollision>();
        collisionChecker.Initialize(TankId, projectileData);
        moveScript.Launch(startPos, endPos, archedTrajectory ? UpperCannonHeight : 0f, 10f / projectileData.Speed);
        if (TankId == 1) moveScript.FlipX();
        if (projectile.TryGetComponent(out AmmoRotate rotateScript)) rotateScript.ReverseRotation();
        yield return null;
    }

    private void TakeDamage(int playerId, int damage) {
        if (TankId != playerId) return;
        TankHealth = Math.Clamp(TankHealth - damage, 0, TankMaxHealth);
        TankDelegates.InvokeOnUpdateTankHealthUI(playerId, TankHealth);
        // Check for game end condition
        TankBattleDelegates.InvokeOnCheckIfBattleIsOver();
    }

    private void InitTank() {
        if (EnemyAI) StartCoroutine(EnemyAICoroutine());
    }
}