using System;
using UnityEngine;

public class PlayerBattleUIDelegates {
    #region Funcs

    public static Func<RectTransform> GetDragLayerRectTransform;
    public static Func<Transform> GetDamageIndicatorTransform;
    public static Func<ulong, Vector3> GetHealthNumberUIPosition;

    #endregion

    #region Events

    public static event Action<Transform, bool> OnDropIndicatorSetParent;
    public static event Action<bool> OnSetDropIndicatorActive;
    public static event Action<int> OnSetDropIndicatorSiblingIndex;
    public static event Action<bool, AmmoData> OnSetAmmoDestinationSlot;

    public static event Action OnCheckForUpgradesSetIcons, OnResetAllAmmoSlotsCanvasGroupAlpha;

    public static void InvokeOnSetAmmoDestinationSlot(bool isUpperCannon, AmmoData ammoData) {
        OnSetAmmoDestinationSlot?.Invoke(isUpperCannon, ammoData);
    }

    public static void InvokeOnCheckForUpgradesSetIcons() {
        OnCheckForUpgradesSetIcons?.Invoke();
    }

    public static void InvokeOnResetAllAmmoSlotsCanvasGroupAlpha() {
        OnResetAllAmmoSlotsCanvasGroupAlpha?.Invoke();
    }

    public static void InvokeOnSetDropIndicatorSiblingIndex(int index) {
        OnSetDropIndicatorSiblingIndex?.Invoke(index);
    }


    public static void InvokeOnSetDropIndicatorActive(bool active) {
        OnSetDropIndicatorActive?.Invoke(active);
    }

    public static void InvokeOnDropIndicatorSetParent(Transform target, bool worldPositionStays) {
        OnDropIndicatorSetParent?.Invoke(target, worldPositionStays);
    }

    #endregion
}