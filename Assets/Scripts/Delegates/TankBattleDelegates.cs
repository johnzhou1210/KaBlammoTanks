using System;
using UnityEngine;

public class TankBattleDelegates
{
    #region Events

    public static event Action CheckIfBattleIsOver;

    public static void InvokeOnCheckIfBattleIsOver() {
        CheckIfBattleIsOver?.Invoke();
    }

    #endregion

    #region Funcs

    #endregion

}
