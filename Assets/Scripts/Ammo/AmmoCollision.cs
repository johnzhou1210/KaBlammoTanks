using System;
using System.Collections;
using System.ComponentModel;
using KBCore.Refs;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public struct AmmoCollisionData : INetworkSerializable {
    public bool CanCollide;
    public int Durability;
    public SpecialEffect SpecialEffect;
    public int Damage;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref CanCollide);
        serializer.SerializeValue(ref Durability);
        serializer.SerializeValue(ref Damage);
        serializer.SerializeValue(ref SpecialEffect);
    }
}

public class AmmoCollision : NetworkBehaviour {
    public ulong OwnerId = 0;
    [SerializeField] private float cleanupTime = 10f, fadeTime = 3f;
    [SerializeField] private int fadeSteps = 15;
    [SerializeField, Child] private TextMeshPro debugNumber;
    public NetworkVariable<AmmoCollisionData> ProjectileCollisionData;
    private NetworkVariable<int> _durability = new NetworkVariable<int>();
    private AudioClip _collisionSound;
    private void OnValidate() {
        this.ValidateRefs();
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if (!ProjectileCollisionData.Value.CanCollide)
            return;
        if (transform.parent != other.transform.parent)
            return;
        AmmoCollision otherAmmo = other.GetComponent<AmmoCollision>();
        if (otherAmmo == null)
            return;
        if (otherAmmo.OwnerId == OwnerId)
            return;
        ExchangeBlows(otherAmmo);
    }
    public void Initialize(ulong ownerId, AmmoData projectileData) {
        OwnerId = ownerId;
        _collisionSound = projectileData.AmmoImpactSound;
        // Turn projectile data into struct for ease of networking
        AmmoCollisionData collisionData = new AmmoCollisionData { CanCollide = projectileData.CanCollide, Durability = projectileData.Durability, SpecialEffect = projectileData.SpecialEffect, Damage = projectileData.Damage };
        ProjectileCollisionData.Value = collisionData;
        _durability.OnValueChanged += (oldVal, newVal) => { UpdateDebugTextClientRpc(newVal, ProjectileCollisionData.Value.Durability); };
        if (IsServer) {
            _durability.Value = ProjectileCollisionData.Value.Durability;
        }
        UpdateDebugTextClientRpc(ProjectileCollisionData.Value.Durability, ProjectileCollisionData.Value.Durability);
    }
    [ClientRpc]
    private void UpdateDebugTextClientRpc(int value, int maxValue) {
        debugNumber.text = $"{value} / {maxValue}";
    }
    public void ExchangeBlows(AmmoCollision otherAmmo) {
        if (!IsServer)
            return;
        int thisOriginalDurability = _durability.Value;
        int otherOriginalDurability = otherAmmo._durability.Value;
        _durability.Value = Math.Clamp(thisOriginalDurability - otherOriginalDurability, 0, ProjectileCollisionData.Value.Durability);
        otherAmmo._durability.Value = Math.Clamp(otherOriginalDurability - thisOriginalDurability, 0, otherAmmo.ProjectileCollisionData.Value.Durability);
        otherAmmo.Collide();
        Collide();
        if (ProjectileCollisionData.Value.SpecialEffect != SpecialEffect.NONE) {
            Vector3 midpoint = (transform.position + otherAmmo.transform.position) * 0.5f;
            GameObject specialEffect = Instantiate(Resources.Load<GameObject>($"Prefabs/VFX/{QuickUtils.GetVFXNameFromEnum(ProjectileCollisionData.Value.SpecialEffect)}"), midpoint - Vector3.forward, Quaternion.identity);
            NetworkObject specialEffectNetworkObject = specialEffect.GetComponent<NetworkObject>();
            specialEffectNetworkObject.Spawn();
        }
    }
    public void Collide(bool hitTank = false, bool forceSparksIfNotExplosive = false) {
        if (IsServer) {
            if (hitTank || _durability.Value == 0) {
                WreckAmmo(forceSparksIfNotExplosive);
            }
            if (hitTank) {
                // Deal damage to tank
            }
        }
        if (IsClient) {
            PlayCollisionSFXClientRpc();
            if (hitTank) {
                // Show damage indicator. put this into a client rpc
                DamageIndicatorClientRpc(OwnerId == 0 ? (ulong)TankDelegates.GetHosteeId?.Invoke()! : 0, ProjectileCollisionData.Value.Damage);
            }
        }
    }
    [ClientRpc]
    private void PlayCollisionSFXClientRpc() {
        AudioManager.Instance.PlaySFXAtPointUI(_collisionSound, Random.Range(0.8f, 1.2f));
    }
    [ClientRpc]
    private void DamageIndicatorClientRpc(ulong targetClientId, int damage, ClientRpcParams clientRpcParams = default) {
        GameObject damageIndicatorPrefab = Instantiate(Resources.Load<GameObject>("Prefabs/UI/DamageIndicator"), PlayerBattleUIDelegates.GetDamageIndicatorTransform?.Invoke());
        Vector3 damageIndicatorScreenPosition = ArenaUIManager.Instance.GetMainCamera().WorldToScreenPoint(transform.position);
        damageIndicatorScreenPosition.z = 0;
        damageIndicatorPrefab.transform.position = damageIndicatorScreenPosition;
        Debug.Log(ProjectileCollisionData);
        Debug.Log(ProjectileCollisionData.Value.Damage);
        Debug.Log(OwnerId);
        damageIndicatorPrefab.GetComponent<DamageIndicator>().Initialize(ProjectileCollisionData.Value.Damage, targetClientId);
    }
    public void WreckAmmo(bool forceSparksIfNotExplosive = false) {
        if (!IsServer)
            return;
        if (QuickUtils.GetIsExplosiveFromSpecialEffectEnum(ProjectileCollisionData.Value.SpecialEffect)) {
            Disintegrate();
        } else {
            transform.parent = AirFieldDelegates.GetDebrisTransform?.Invoke();
            DebrifyClientRpc();
            if (ProjectileCollisionData.Value.SpecialEffect != SpecialEffect.NONE) {
                GameObject effect = Instantiate(Resources.Load<GameObject>($"Prefabs/VFX/{QuickUtils.GetVFXNameFromEnum(ProjectileCollisionData.Value.SpecialEffect)}"), transform.position - Vector3.forward, Quaternion.identity);
                NetworkObject effectNetworkObject = effect.GetComponent<NetworkObject>();
                effectNetworkObject.Spawn();
            }
            Rigidbody2D rigidbody = gameObject.GetComponent<Rigidbody2D>();
            rigidbody.AddForce(Vector3.left * (OwnerId == 0 ? 1f : -1f), ForceMode2D.Impulse);
        }
    }
    private void Disintegrate() {
        GameObject explosion = Instantiate(Resources.Load<GameObject>($"Prefabs/VFX/{QuickUtils.GetVFXNameFromEnum(ProjectileCollisionData.Value.SpecialEffect)}"), transform.position - Vector3.forward, Quaternion.identity);
        NetworkObject explosionNetworkObject = explosion.GetComponent<NetworkObject>();
        explosionNetworkObject.Spawn();
        Destroy(gameObject);
    }
    [ClientRpc]
    private void DebrifyClientRpc() {
        BezierProjectile bezierScript = gameObject.GetComponent<BezierProjectile>();
        AmmoRotate ammoRotateScript = gameObject.GetComponent<AmmoRotate>();
        if (bezierScript != null)
            bezierScript.enabled = false;
        if (ammoRotateScript)
            ammoRotateScript.enabled = false;
        Rigidbody2D rigidbody = gameObject.GetComponent<Rigidbody2D>();
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
        rigidbody.gravityScale = 0.5f;
        rigidbody.mass = 1.2f;
        gameObject.GetComponent<Collider2D>().isTrigger = false;
        StartCoroutine(Cleanup());
    }
    private IEnumerator Cleanup() {
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        float originalAlpha = spriteRenderer.color.a;
        spriteRenderer.color *= 0.5f;
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, originalAlpha);
        yield return new WaitForSeconds(cleanupTime);
        float alphaToDecrease = spriteRenderer.color.a;
        if (alphaToDecrease <= 0)
            yield break;
        for (int i = 0; i < fadeSteps; i++) {
            float decreaseAmount = alphaToDecrease / fadeSteps;
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, spriteRenderer.color.a - decreaseAmount);
            yield return new WaitForSeconds(fadeTime / fadeSteps);
        }
        DestroyProjectileServerRpc();
        yield return null;
    }

    [ServerRpc (RequireOwnership = false)]
    private void DestroyProjectileServerRpc() {
        Destroy(gameObject);
    }
}
