using System;
using System.Collections;
using KBCore.Refs;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class AmmoCollision : NetworkBehaviour {
    public ulong OwnerId = 0;
    public AmmoData ProjectileData;
    [SerializeField] private float cleanupTime = 10f, fadeTime = 3f;
    [SerializeField] private int fadeSteps = 15;
    [SerializeField, Child] private TextMeshPro debugNumber;
    
    private int _durability = 1;


    private void OnValidate() {
        this.ValidateRefs();
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if (!ProjectileData.CanCollide) return;
        if (transform.parent != other.transform.parent) return;
        AmmoCollision otherAmmo = other.GetComponent<AmmoCollision>();
        if (otherAmmo == null) return;
        if (otherAmmo.OwnerId == OwnerId) return;
        ExchangeBlows(otherAmmo);
    }

    public void Initialize(ulong ownerId, AmmoData projectileData) {
        OwnerId = NetworkManager.Singleton.LocalClientId;
        ProjectileData = projectileData;
        _durability = ProjectileData.Durability;
        debugNumber.text = $"{_durability} / {ProjectileData.Durability}";
    }

    public void ExchangeBlows(AmmoCollision otherAmmo) {
        int thisOriginalDurability = _durability;
        int otherOriginalDurability = otherAmmo._durability;

        _durability = Math.Clamp(thisOriginalDurability - otherOriginalDurability, 0, ProjectileData.Durability);
        otherAmmo._durability = Math.Clamp(otherOriginalDurability - thisOriginalDurability, 0, otherAmmo.ProjectileData.Durability);
        // Debug.Log($"{_durability} / {ProjectileData.Durability}");
        debugNumber.text = $"{_durability} / {ProjectileData.Durability}";
        otherAmmo.debugNumber.text = $"{otherAmmo._durability} / {otherAmmo.ProjectileData.Durability}";
        otherAmmo.Collide();
        Collide();
        if (!ProjectileData.Explosive) {
            Vector3 midpoint = (transform.position + otherAmmo.transform.position) * 0.5f;
            Instantiate(Resources.Load<GameObject>("Prefabs/VFX/CollisionEffect"), midpoint, Quaternion.identity);
        }
        
    }

    public void Collide(bool hitTank = false, bool forceSparksIfNotExplosive = false) {
        if (hitTank || _durability == 0) {
            WreckAmmo(forceSparksIfNotExplosive);
        }
        if (hitTank) {
            // Show damage indicator
            GameObject DamageIndicatorPrefab = Instantiate(Resources.Load<GameObject>("Prefabs/UI/DamageIndicator"), PlayerBattleUIDelegates.GetDamageIndicatorTransform?.Invoke());
            Vector3 DamageIndicatorScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
            DamageIndicatorScreenPosition.z = 0;
            DamageIndicatorPrefab.transform.position = DamageIndicatorScreenPosition;
            DamageIndicatorPrefab.GetComponent<DamageIndicator>().Initialize(ProjectileData.Damage, OwnerId == 0 ?  (ulong) 1 : 0 );
        }
    }


    private void WreckAmmo(bool forceSparksIfNotExplosive = false) {
        if (ProjectileData.Explosive) {
            Disintegrate();
        } else {
            Debrify();
            if (forceSparksIfNotExplosive) {
                Instantiate(Resources.Load<GameObject>("Prefabs/VFX/CollisionEffect"), transform.position, Quaternion.identity);
            }
            Rigidbody2D rigidbody = gameObject.GetComponent<Rigidbody2D>();
            rigidbody.AddForce(Vector3.left * (OwnerId == 0 ? 1f : -1f), ForceMode2D.Impulse);
        }
    }

    private void Disintegrate() {
        Instantiate(Resources.Load<GameObject>("Prefabs/VFX/ExplosionEffect"), transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void Debrify() {
        BezierProjectile bezierScript = gameObject.GetComponent<BezierProjectile>();
        AmmoRotate ammoRotateScript = gameObject.GetComponent<AmmoRotate>();
        if (bezierScript != null) bezierScript.enabled = false;
        if (ammoRotateScript) ammoRotateScript.enabled = false;

        Rigidbody2D rigidbody = gameObject.GetComponent<Rigidbody2D>();
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
        rigidbody.gravityScale = 0.5f;
        rigidbody.mass = 1.2f;
        
        gameObject.GetComponent<Collider2D>().isTrigger = false;
        


        transform.parent = AirFieldDelegates.GetDebrisTransform?.Invoke();

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

        Destroy(gameObject);

        yield return null;
    }
}