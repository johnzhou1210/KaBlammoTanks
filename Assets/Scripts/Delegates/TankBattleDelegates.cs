using System;
using UnityEngine;

public class TankBattleDelegates
{
    #region Events

    public static event Action OnCheckIfBattleIsOver;
    public static event Action OnInitTanks;

    public static void InvokeOnCheckIfBattleIsOver() {
        OnCheckIfBattleIsOver?.Invoke();
    }

    public static void InvokeOnInitTanks() {
        OnInitTanks?.Invoke();
    }

    #endregion

    #region Funcs

    #endregion

}
