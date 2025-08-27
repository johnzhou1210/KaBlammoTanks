using System;
using Unity.Netcode;
using UnityEngine;

public class TankDelegates
{
    #region Events

    public static event Action<AmmoRequest, bool> OnProjectileFire;
    public static event Action<ulong, int> OnUpdateTankHealthUI;
    public static event Action<ulong, int> OnTakeDamage;

    public static void InvokeOnProjectileFire(AmmoRequest request, bool isUpperCannon)
    {
        OnProjectileFire?.Invoke(request, isUpperCannon);
    }

    public static void InvokeOnTakeDamage(ulong targetId, int damage)
    {
        OnTakeDamage?.Invoke(targetId, damage);
    }

    public static void InvokeOnUpdateTankHealthUI(ulong playerId, int health)
    {
        OnUpdateTankHealthUI?.Invoke(playerId, health);
    }

    #endregion

    #region Funcs

    public static Func<TankController> GetHostTankController, GetHosteeTankController;
    public static Func<GameObject> GetHostTankGameObject, GetHosteeTankGameObject;
    
    #endregion

}

