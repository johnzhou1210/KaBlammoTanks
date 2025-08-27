using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ammo Database", menuName = "Ammo Database")]
public class AmmoDatabase : ScriptableObject {
    public AmmoData[] AllAmmo;

    private Dictionary<string, AmmoData> lookup;

    private void Awake() {
        lookup = new Dictionary<string, AmmoData>();
        foreach (var ammo in AllAmmo) {
            lookup[ammo.AmmoName] = ammo;
        }
    }

    public AmmoData GetAmmo(string ammoName) {
        return lookup.ContainsKey(ammoName) ? lookup[ammoName] : null;
    }
}
