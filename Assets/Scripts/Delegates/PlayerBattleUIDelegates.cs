using System;
using UnityEngine;

public class PlayerBattleUIDelegates {
    public static event Action<TitleDamagePair> OnDescriptionDataChanged;
    public static event Action<bool, AmmoData> OnSetAmmoDestinationSlot;

    public static void InvokeOnShopItemDescriptionChanged(TitleDamagePair descriptionInfo) {
        OnDescriptionDataChanged?.Invoke(descriptionInfo);
    }

    public static void InvokeOnSetAmmoDestinationSlot(bool isUpperCannon, AmmoData ammoData) {
        OnSetAmmoDestinationSlot?.Invoke(isUpperCannon, ammoData);
    }


}
