using System;
using System.Collections;
using KBCore.Refs;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public struct AmmoCollisionData : INetworkSerializable {
    public bool CanCollide;
    public int Durability;
    public bool Explosive;
    public int Damage;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref CanCollide);
        serializer.SerializeValue(ref Durability);
        serializer.SerializeValue(ref Explosive);
        serializer.SerializeValue(ref Damage);
    }
}

public class AmmoCollision : NetworkBehaviour {
    public ulong OwnerId = 0;
    
    [SerializeField] private float cleanupTime = 10f, fadeTime = 3f;
    [SerializeField] private int fadeSteps = 15;
    [SerializeField, Child] private TextMeshPro debugNumber;
    
    public NetworkVariable<AmmoCollisionData> ProjectileCollisionData;
    private NetworkVariable<int> _durability = new NetworkVariable<int>();


    private void OnValidate() {
        this.ValidateRefs();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (!ProjectileCollisionData.Value.CanCollide) return;
        if (transform.parent != other.transform.parent) return;
        AmmoCollision otherAmmo = other.GetComponent<AmmoCollision>();
        if (otherAmmo == null) return;
        if (otherAmmo.OwnerId == OwnerId) return;
        ExchangeBlows(otherAmmo);
    }

    public void Initialize(ulong ownerId, AmmoData projectileData) {
        OwnerId = ownerId;
        
        // Turn projectile data into struct for ease of networking
        AmmoCollisionData collisionData = new AmmoCollisionData {
            CanCollide = projectileData.CanCollide,
            Durability = projectileData.Durability,
            Explosive = projectileData.Explosive,
            Damage = projectileData.Damage
        };
        ProjectileCollisionData.Value = collisionData;
        _durability.OnValueChanged += (oldVal, newVal) => {
            UpdateDebugTextClientRpc(newVal, ProjectileCollisionData.Value.Durability);
        };

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
        if (!IsServer) return;
        int thisOriginalDurability = _durability.Value;
        int otherOriginalDurability = otherAmmo._durability.Value;
        _durability.Value = Math.Clamp(thisOriginalDurability - otherOriginalDurability, 0, ProjectileCollisionData.Value.Durability);
        otherAmmo._durability.Value = Math.Clamp(otherOriginalDurability - thisOriginalDurability, 0, otherAmmo.ProjectileCollisionData.Value.Durability);
        otherAmmo.Collide();
        Collide();
        if (!ProjectileCollisionData.Value.Explosive) {
            Vector3 midpoint = (transform.position + otherAmmo.transform.position) * 0.5f;
            GameObject collisionEffect = Instantiate(Resources.Load<GameObject>("Prefabs/VFX/CollisionEffect"), midpoint, Quaternion.identity);
            NetworkObject collisionEffectNetworkObject = collisionEffect.GetComponent<NetworkObject>();
            collisionEffectNetworkObject.Spawn();
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
            if (hitTank) {
                // Show damage indicator. put this into a client rpc
                DamageIndicatorClientRpc(OwnerId == 0 ? (ulong) 1 : 0, ProjectileCollisionData.Value.Damage);
            }
        }
        
    }

    [ClientRpc]
    private void DamageIndicatorClientRpc(ulong targetClientId, int damage, ClientRpcParams clientRpcParams = default) {
        GameObject damageIndicatorPrefab = Instantiate(Resources.Load<GameObject>("Prefabs/UI/DamageIndicator"), PlayerBattleUIDelegates.GetDamageIndicatorTransform?.Invoke());
        Vector3 damageIndicatorScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
        damageIndicatorScreenPosition.z = 0;
        damageIndicatorPrefab.transform.position = damageIndicatorScreenPosition;
        Debug.Log(ProjectileCollisionData);
        Debug.Log(ProjectileCollisionData.Value.Damage);
        Debug.Log(OwnerId);
        damageIndicatorPrefab.GetComponent<DamageIndicator>().Initialize(ProjectileCollisionData.Value.Damage, targetClientId);
    }

    private void WreckAmmo(bool forceSparksIfNotExplosive = false) {
        if (!IsServer) return;
        if (ProjectileCollisionData.Value.Explosive) {
            Disintegrate();
        } else {
            transform.parent = AirFieldDelegates.GetDebrisTransform?.Invoke();
            DebrifyClientRpc();
            if (forceSparksIfNotExplosive) {
                GameObject sparks = Instantiate(Resources.Load<GameObject>("Prefabs/VFX/CollisionEffect"), transform.position, Quaternion.identity);
                NetworkObject sparksNetworkObject = sparks.GetComponent<NetworkObject>();
                sparksNetworkObject.Spawn();
            }
            Rigidbody2D rigidbody = gameObject.GetComponent<Rigidbody2D>();
            rigidbody.AddForce(Vector3.left * (OwnerId == 0 ? 1f : -1f), ForceMode2D.Impulse);
        }
    }

    private void Disintegrate() {
        GameObject explosion = Instantiate(Resources.Load<GameObject>("Prefabs/VFX/ExplosionEffect"), transform.position, Quaternion.identity);
        NetworkObject explosionNetworkObject = explosion.GetComponent<NetworkObject>();
        explosionNetworkObject.Spawn();
        
        Destroy(gameObject);
    }

    [ClientRpc]
    private void DebrifyClientRpc() {
        BezierProjectile bezierScript = gameObject.GetComponent<BezierProjectile>();
        AmmoRotate ammoRotateScript = gameObject.GetComponent<AmmoRotate>();
        if (bezierScript != null) bezierScript.enabled = false;
        if (ammoRotateScript) ammoRotateScript.enabled = false;

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
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b,
            originalAlpha);
        yield return new WaitForSeconds(cleanupTime);
        float alphaToDecrease = spriteRenderer.color.a;
        if (alphaToDecrease <= 0) yield break;
        for (int i = 0; i < fadeSteps; i++) {
            float decreaseAmount = alphaToDecrease / fadeSteps;
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b,
                spriteRenderer.color.a - decreaseAmount);
            yield return new WaitForSeconds(fadeTime / fadeSteps);
        }

        if (IsServer) {
            Destroy(gameObject);
        }
        

        yield return null;
    }
}