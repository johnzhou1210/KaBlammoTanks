using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBattleInputDelegates {
    #region Events
    public static event Action<AmmoSlot> OnShopAmmoTap;
    public static event Action OnRemoveActiveAmmoShopItem;

    public static void InvokeOnShopAmmoTap(AmmoSlot ammoSlot) {
        OnShopAmmoTap?.Invoke(ammoSlot);
    }

    public static void InvokeOnRemoveActiveAmmoShopItem() {
        OnRemoveActiveAmmoShopItem?.Invoke();
    }
    #endregion
    
    
    
    #region Funcs

    public static Func<AmmoSlot> GetSelectedAmmoShopItem;

    #endregion

}
