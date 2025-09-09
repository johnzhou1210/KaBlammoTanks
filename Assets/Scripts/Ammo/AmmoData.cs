using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct UpgradeRecipe {
   public AmmoData CombineWith;
   public AmmoData UpgradesTo;
}

public enum Rarity {
   COMMON,
   RARE,
   EPIC,
   LEGENDARY,
}

public enum SpecialEffect {
   NONE,
   COLLISION,
   SMALL_EXPLOSION,
   LARGE_EXPLOSION
}

[CreateAssetMenu(fileName = "New Ammo", menuName = "Ammo", order = 1)]
public class AmmoData : ScriptableObject {
   public string AmmoName;
   public Sprite Icon;
   public int Damage;
   public float Speed;
   public GameObject ProjectilePrefab;
   public int Cost;
   public float LoadingTime;
   public UpgradeRecipe UpgradeRecipe;
   public Rarity Rarity;
   public bool CanCollide;
   public int Durability = 1;
   public AudioClip AmmoImpactSound;
   public SpecialEffect SpecialEffect;
   
   
   public override string ToString() {
      return $"[AmmoData] " +
             $"Name: {AmmoName}, " +
             $"Damage: {Damage}, " +
             $"Speed: {Speed}, " +
             $"Cost: {Cost}, " +
             $"Durability: {Durability}, " +
             $"Effect: {SpecialEffect}, " +
             $"Rarity: {Rarity}, " +
             $"Collidable: {CanCollide}";
   }
   
}

public struct AmmoRequest : INetworkSerializable {
   public string AmmoName;
   public bool IsValid;

   public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
      serializer.SerializeValue(ref AmmoName);
      
   }
}

