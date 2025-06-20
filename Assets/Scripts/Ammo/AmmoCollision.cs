using System.Collections;
using UnityEngine;

public class AmmoCollision : MonoBehaviour {
    public int OwnerId = -1;
    public AmmoData ProjectileData;
    public bool Explosive;
    [SerializeField] private float cleanupTime = 10f, fadeTime = 3f;
    [SerializeField] private int fadeSteps = 15;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.transform.parent != AirFieldDelegates.GetAirFieldTransform?.Invoke()) return;
        AmmoCollision otherAmmo = other.GetComponent<AmmoCollision>();
        if (otherAmmo == null) return;
        if (otherAmmo.OwnerId == OwnerId) return;
        otherAmmo.Collide();
        Collide();
    }

    public void Collide() {
        if (Explosive)
            Disintegrate();
        else
            Debrify();
        Rigidbody2D rigidbody = gameObject.GetComponent<Rigidbody2D>();
        rigidbody.AddForce(Vector3.left * (OwnerId == 0 ? 1f : -1f), ForceMode2D.Impulse);
    }

    public void Disintegrate() {
        Instantiate(Resources.Load<GameObject>("Prefabs/VFX/ExplosionEffect"), transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void Debrify() {
        BezierProjectile bezierScript = gameObject.GetComponent<BezierProjectile>();
        AmmoRotate ammoRotateScript = gameObject.GetComponent<AmmoRotate>();
        if (bezierScript != null) bezierScript.enabled = false;
        if (ammoRotateScript) ammoRotateScript.enabled = false;


        gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
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