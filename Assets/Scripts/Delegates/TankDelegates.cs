using System;
using Unity.Netcode;
using UnityEngine;

public class TankDelegates
{
    #region Events

    public static event Action<AmmoRequest, bool> OnProjectileFire;
    public static event Action<ulong, int> OnUpdateTankHealthUI;
    public static event Action<int, ulong> OnTakeDamage;

    public static void InvokeOnProjectileFire(AmmoRequest request, bool isUpperCannon)
    {
        OnProjectileFire?.Invoke(request, isUpperCannon);
    }

    public static void InvokeOnTakeDamage(int damage, ulong targetId)
    {
        Debug.Log($"[TankDelegates] Invoking OnTakeDamage damage={damage} targetId={targetId}");
        OnTakeDamage?.Invoke(damage, targetId);
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

