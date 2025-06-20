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
}
