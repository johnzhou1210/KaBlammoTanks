using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBattleInputDelegates {
    #region Events
    public static event Action<AmmoTapHandler> OnShopAmmoTap;

    public static void InvokeOnShopAmmoTap(AmmoTapHandler ammoTapHandler) {
        OnShopAmmoTap?.Invoke(ammoTapHandler);
    }
    #endregion
    
    
    
    #region Funcs

    public static Func<AmmoTapHandler> GetSelectedAmmoShopItem;

    #endregion

}
