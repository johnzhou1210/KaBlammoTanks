using System;
using KBCore.Refs;
using UnityEngine;

public class DamageIndicatorManager : MonoBehaviour {

    private void OnEnable() {
        PlayerBattleUIDelegates.GetDamageIndicatorTransform = () => transform;
    }

    private void OnDisable() {
        PlayerBattleUIDelegates.GetDamageIndicatorTransform = null;
    }
}
