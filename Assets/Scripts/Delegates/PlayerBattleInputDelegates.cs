using System;
using System.Collections.Generic;

public class PlayerBattleInputDelegates {
    #region Events

    public static event Action<AmmoSlot> OnShopAmmoTap;
    public static event Action OnRemoveActiveAmmoShopItem, OnDelayedCheckForUpgrades;


    public static void InvokeOnShopAmmoTap(AmmoSlot ammoSlot) {
        OnShopAmmoTap?.Invoke(ammoSlot);
    }

    public static void InvokeOnRemoveActiveAmmoShopItem() {
        OnRemoveActiveAmmoShopItem?.Invoke();
    }

    public static void InvokeOnDelayedCheckForUpgrades() {
        OnDelayedCheckForUpgrades?.Invoke();
    }

    #endregion


    #region Funcs

    public static Func<AmmoSlot> GetSelectedAmmoShopItem;
    public static Func<List<AmmoSlot>> GetAllAmmoSlots;

    #endregion
}