using UnityEngine;

[CreateAssetMenu(fileName = "New Ammo", menuName = "Ammo", order = 1)]
public class AmmoData : ScriptableObject {
   public string AmmoName;
   public Sprite Icon;
   public int Damage;
   public float Speed;
   public GameObject ProjectilePrefab;
   public int Cost;
   public float LoadingTime;
}
