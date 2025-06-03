using System;
using UnityEngine;

public class AmmoCollision : MonoBehaviour {
   public int OwnerId = -1;
   public AmmoData ProjectileData;

   void OnTriggerEnter2D(Collider2D other) {
      AmmoCollision otherAmmo = other.GetComponent<AmmoCollision>();
      if (otherAmmo == null) return;
      if (otherAmmo.OwnerId == OwnerId) return;
      otherAmmo.Disintegrate();
      Disintegrate();
   }

   public void Disintegrate() {
      Instantiate(Resources.Load<GameObject>("Prefabs/VFX/ExplosionEffect"), transform.position, Quaternion.identity);
      Destroy(gameObject);
   }

}
