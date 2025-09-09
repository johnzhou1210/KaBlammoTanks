using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "New Ammo Database", menuName = "Ammo Database")]
public class AmmoDatabase : ScriptableObject {
    public List<AmmoEntry> AllAmmo;

    private Dictionary<string, AmmoData> lookup;

    private void Awake() {
        lookup = new Dictionary<string, AmmoData>();
        foreach (var ammo in AllAmmo) {
            lookup[ammo.AmmoData.AmmoName] = ammo.AmmoData;
        }
    }

    public AmmoData GetAmmo(string ammoName) {
        return lookup.ContainsKey(ammoName) ? lookup[ammoName] : null;
    }

    public AmmoData GetRandomAmmoWeighted() {
        // Calculate total weight first
        int totalWeight = AllAmmo.Sum(entry => entry.Weight);
        float randomVal = Random.Range(0f, totalWeight);

        int cumulative = 0;
        foreach (var entry in AllAmmo) {
            cumulative += entry.Weight;
            if (randomVal <= cumulative) {
                return entry.AmmoData;
            }
        }
        return AllAmmo[0].AmmoData;
    }
    
    public static AmmoRequest GetAmmoRequest(AmmoData ammoData) {
        return new AmmoRequest {
            AmmoName = ammoData.AmmoName,
            IsValid = true
        };
    }
}

[System.Serializable]
public class AmmoEntry {
    public AmmoData AmmoData;
    public int Weight;
}
