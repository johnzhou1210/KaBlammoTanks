using System;
using System.Collections;
using System.Collections.Generic;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AmmoSlot))]
public class AmmoUpgradeCombinerHandler : MonoBehaviour {
    [SerializeField, Self] private AmmoSlot ammoSlot;
    private Color _originalColor;
    private Coroutine _autoUpgradeCoroutine;
    private void OnValidate() {
        this.ValidateRefs();
    }
    private void OnEnable() {
        _originalColor = GetComponent<Image>().color;
        PlayerBattleUIDelegates.OnDoAutoUpgrades += DoAutoUpgrades;
    }
    private void OnDisable() {
        RestoreOriginalColor();
        PlayerBattleUIDelegates.OnDoAutoUpgrades -= DoAutoUpgrades;
    }
    private float GetPitchBasedOnResultRarity(Rarity rarity) {
        switch (rarity) {
            case Rarity.COMMON: return .8f;
            case Rarity.RARE: return .9f;
            case Rarity.EPIC: return 1f;
            case Rarity.LEGENDARY: return 1.1f;
            default: return 0f;
        }
    }
    private void DoAutoUpgrades() {
        if (_autoUpgradeCoroutine != null) {
            StopCoroutine(_autoUpgradeCoroutine);
            _autoUpgradeCoroutine = null;
        }
        _autoUpgradeCoroutine = StartCoroutine(AutoUpgradeCoroutine());
    }
    private List<AmmoSlot> GetAllAmmoSlots() {
        return PlayerBattleInputDelegates.GetAllAmmoSlots?.Invoke();
    }
    private bool CanAutoUpgrade() {
        List<AmmoSlot> allAmmoSlots = GetAllAmmoSlots();
        if (allAmmoSlots == null) {
            Debug.LogWarning("AllAmmoSlots is null!");
            return false;
        }
        if (GetAllAmmoSlots().Count <= 1)
            return false;
        for (int i = 0; i < allAmmoSlots.Count; i++) {
            AmmoSlot currAmmoSlot = allAmmoSlots[i];
            AmmoSlot prevAmmoSlot = (i > 0) ? allAmmoSlots[i - 1] : null;
            AmmoSlot nextAmmoSlot = (i < allAmmoSlots.Count - 1) ? allAmmoSlots[i + 1] : null;
            AmmoData neededAmmoToUpgrade = currAmmoSlot.AmmoData.UpgradeRecipe.CombineWith;
            if (neededAmmoToUpgrade == null)
                continue;
            if (prevAmmoSlot != null && currAmmoSlot.AmmoData == neededAmmoToUpgrade) {
                if (prevAmmoSlot.AmmoData.UpgradeRecipe.CombineWith == null)
                    continue;
                if (prevAmmoSlot.AmmoData.UpgradeRecipe.CombineWith != currAmmoSlot.AmmoData.UpgradeRecipe.CombineWith)
                    continue;
                CombineAmmoSlots(prevAmmoSlot, currAmmoSlot);
                return true;
            }
            if (nextAmmoSlot != null && nextAmmoSlot.AmmoData == neededAmmoToUpgrade) {
                if (nextAmmoSlot.AmmoData.UpgradeRecipe.CombineWith == null)
                    continue;
                if (nextAmmoSlot.AmmoData.UpgradeRecipe.CombineWith != currAmmoSlot.AmmoData.UpgradeRecipe.CombineWith)
                    continue;
                CombineAmmoSlots(currAmmoSlot, nextAmmoSlot);
                return true;
            }
        }
        return false;
    }
    /* Merges left slot into right slot, effectively removing left slot and storing the result in right slot. */
    private void CombineAmmoSlots(AmmoSlot left, AmmoSlot right) {
        Debug.Log("Attempting to merge " + left.AmmoData.AmmoName + " into " + right.AmmoData.AmmoName);
        if (left == null || right == null) {
            Debug.LogError("Combine failed because left slot or right slot is null.");
            return;
        }
        if (left.AmmoData.UpgradeRecipe.CombineWith != right.AmmoData.UpgradeRecipe.CombineWith) {
            Debug.LogError("The two slots cannot be combined because the recipe does not match!");
            return;
        }

        // Attempt to combine
        AmmoData resultAmmo = right.AmmoData.UpgradeRecipe.UpgradesTo;
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/SFX/AmmoCombine"), GetPitchBasedOnResultRarity(resultAmmo.Rarity));

        // Remove the left ammo slot, and update the right ammo slot
        Destroy(left.gameObject);
        right.SetSlotData(resultAmmo);

        // Reassess upgrade available for all ammo
        PlayerBattleUIDelegates.InvokeOnCheckForUpgradesSetIcons();
        PlayerBattleUIDelegates.InvokeOnResetAllAmmoSlotsCanvasGroupAlpha();
    }
    private IEnumerator AutoUpgradeCoroutine() {
        while (CanAutoUpgrade()) {
            Debug.Log("Combining ammo");
            yield return null;
        }
    }

    // public void OnPointerEnter(PointerEventData eventData) {
    //     print("Pointer enter ");
    //     GetComponent<Image>().color = Color.white;
    //     
    //     // Disable ghost slot
    //     PlayerBattleUIDelegates.InvokeOnSetDropIndicatorActive(false);
    // }
    //
    // public void OnPointerExit(PointerEventData eventData) {
    //     print("Pointer exit");
    //     RestoreOriginalColor();
    //     
    //     // Reenable ghost slot
    //     PlayerBattleUIDelegates.InvokeOnSetDropIndicatorActive(true);
    // }
    private void RestoreOriginalColor() {
        GetComponent<Image>().color = _originalColor;
    }
}
