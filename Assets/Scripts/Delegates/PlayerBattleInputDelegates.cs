using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBattleInputDelegates {
    #region Events

    public static event Action<AmmoSlot> OnShopAmmoTap;
    public static event Action OnRemoveActiveAmmoShopItem, OnDelayedCheckForUpgrades, OnRequestUpgradeCheck;


    public static void InvokeOnShopAmmoTap(AmmoSlot ammoSlot) {
        OnShopAmmoTap?.Invoke(ammoSlot);
    }

    public static void InvokeOnRemoveActiveAmmoShopItem() {
        OnRemoveActiveAmmoShopItem?.Invoke();
    }

    public static void InvokeOnDelayedCheckForUpgrades() {
        OnDelayedCheckForUpgrades?.Invoke();
    }

    public static void InvokeOnRequestUpgradeCheck() {
        OnRequestUpgradeCheck?.Invoke();
    }

    #endregion


    #region Funcs

    public static Func<GameObject> GetAmmoSlotContainer;
    public static Func<AmmoSlot> GetSelectedAmmoShopItem;
    public static Func<List<AmmoSlot>> GetAllAmmoSlots;

    #endregion
}