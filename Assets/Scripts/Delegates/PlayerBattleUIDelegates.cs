using System;
using UnityEngine;

public class PlayerBattleUIDelegates {

    #region Events
    
    public static event Action<bool, AmmoData> OnSetAmmoDestinationSlot;
    public static event Action<bool, AmmoData> OnSetCombinerListenerEnabled;
    public static event Action OnCheckForUpgradesSetIcons, OnResetAllAmmoSlotsCanvasGroupAlpha;
   
    public static void InvokeOnSetAmmoDestinationSlot(bool isUpperCannon, AmmoData ammoData) {
        OnSetAmmoDestinationSlot?.Invoke(isUpperCannon, ammoData);
    }
    public static void InvokeOnSetCombinerListener(bool val, AmmoData ammoData) {
        OnSetCombinerListenerEnabled?.Invoke(val, ammoData);
    }
    public static void InvokeOnCheckForUpgradesSetIcons() {
        OnCheckForUpgradesSetIcons?.Invoke();
    }
    public static void InvokeOnResetAllAmmoSlotsCanvasGroupAlpha() {
        OnResetAllAmmoSlotsCanvasGroupAlpha?.Invoke();
    }

    #endregion

    #region Funcs

    public static Func<RectTransform> GetDragLayerRectTransform;

    #endregion

}
