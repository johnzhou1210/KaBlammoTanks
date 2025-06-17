using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New Ammo Pool", menuName = "Ammo Pool", order = 1)]
public class AmmoPool : ScriptableObject {
    public AmmoData[] Contents;
}