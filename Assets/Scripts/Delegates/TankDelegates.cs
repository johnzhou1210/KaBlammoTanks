using System;
using UnityEngine;

public class TankDelegates {

    #region Events

    public static event Action<AmmoData, bool, int> OnProjectileFire;
    public static event Action<int, int> OnTakeDamage, OnUpdateTankHealthUI;
    public static void InvokeOnProjectileFire(AmmoData ammoData, bool isUpperCannon, int playerId) {
        OnProjectileFire?.Invoke(ammoData, isUpperCannon, playerId);
    }
    public static void InvokeOnTakeDamage(int playerId, int damage) {
        OnTakeDamage?.Invoke(playerId, damage);
    }

    public static void InvokeOnUpdateTankHealthUI(int playerId, int health) {
        OnUpdateTankHealthUI?.Invoke(playerId, health);
    }

    #endregion

    #region Funcs

    public static Func<int, TankController> GetTankControllerById;
    public static Func<int,int> GetTankHealthById, GetTankMaxHealthById;

    #endregion

}
