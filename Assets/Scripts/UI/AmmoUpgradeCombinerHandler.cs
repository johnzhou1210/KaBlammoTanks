using System.Collections;
using System.Collections.Generic;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AmmoSlot))]
public class AmmoUpgradeCombinerHandler : MonoBehaviour {
    [SerializeField] [Self] private AmmoSlot ammoSlot;
    private Coroutine _autoUpgradeCoroutine;
    private Color _originalColor;

    private void OnEnable() {
        _originalColor = GetComponent<Image>().color;
        PlayerBattleUIDelegates.OnDoAutoUpgrades += DoAutoUpgrades;
    }

    private void OnDisable() {
        RestoreOriginalColor();
        PlayerBattleUIDelegates.OnDoAutoUpgrades -= DoAutoUpgrades;
    }

    private void OnValidate() {
        this.ValidateRefs();
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
        var allAmmoSlots = GetAllAmmoSlots();
        if (allAmmoSlots == null) {
            Debug.LogWarning("AllAmmoSlots is null!");
            return false;
        }

        if (GetAllAmmoSlots().Count <= 1)
            return false;
        for (var i = 0; i < allAmmoSlots.Count; i++) {
            var currAmmoSlot = allAmmoSlots[i];
            var prevAmmoSlot = i > 0 ? allAmmoSlots[i - 1] : null;
            var nextAmmoSlot = i < allAmmoSlots.Count - 1 ? allAmmoSlots[i + 1] : null;
            var neededAmmoToUpgrade = currAmmoSlot.AmmoData.UpgradeRecipe.CombineWith;
            if (neededAmmoToUpgrade == null)
                continue;
            if (prevAmmoSlot != null && currAmmoSlot.AmmoData == neededAmmoToUpgrade) {
                if (prevAmmoSlot.AmmoData.UpgradeRecipe.CombineWith == null)
                    continue;
                if (prevAmmoSlot.AmmoData.UpgradeRecipe.CombineWith != currAmmoSlot.AmmoData.UpgradeRecipe.CombineWith)
                    continue;
                MergeSlotsWithAnimation(prevAmmoSlot, currAmmoSlot);
                return true;
            }

            if (nextAmmoSlot != null && nextAmmoSlot.AmmoData == neededAmmoToUpgrade) {
                if (nextAmmoSlot.AmmoData.UpgradeRecipe.CombineWith == null)
                    continue;
                if (nextAmmoSlot.AmmoData.UpgradeRecipe.CombineWith != currAmmoSlot.AmmoData.UpgradeRecipe.CombineWith)
                    continue;
                MergeSlotsWithAnimation(currAmmoSlot, nextAmmoSlot);
                return true;
            }
        }

        return false;
    }

    /* Merges left slot into right slot, effectively removing left slot and storing the result in right slot. */
    private void MergeSlotsWithAnimation(AmmoSlot left, AmmoSlot right) {
        StartCoroutine(MergeSlotCoroutine(left, right));
    }

    private IEnumerator AutoUpgradeCoroutine() {
        while (CanAutoUpgrade()) {
            Debug.Log("Combining ammo");
            yield return null;
        }
    }

    private IEnumerator MergeSlotCoroutine(AmmoSlot left, AmmoSlot right) {
        var dragLayerTransform = PlayerBattleUIDelegates.GetDragLayerRectTransform?.Invoke();
        if (dragLayerTransform == null) {
            Debug.LogError("Drag Layer transform is null!");
            yield break;
        }

        var leftAnimator = left.GetComponent<Animator>();
        var rightAnimator = right.GetComponent<Animator>();

        left.transform.parent.SetParent(dragLayerTransform, true);
        leftAnimator.Play("AmmoCardMerger");
        rightAnimator.StopPlayback();
        rightAnimator.Play("AmmoCardMergee");

        StartCoroutine(WaitAndCompleteMergeCoroutine(left, right));

        yield return null;
    }

    private IEnumerator WaitAndCompleteMergeCoroutine(AmmoSlot left, AmmoSlot right) {
        yield return new WaitForSeconds(0.25f / .6f);
        var resultAmmo = right.AmmoData.UpgradeRecipe.UpgradesTo;
        // Remove the left ammo slot, and update the right ammo slot
        Destroy(left.transform.parent.gameObject);
        right.SetSlotData(resultAmmo);
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/SFX/AmmoCombine"),
            GetPitchBasedOnResultRarity(resultAmmo.Rarity));
        // Reassess upgrade available for all ammo
        PlayerBattleUIDelegates.InvokeOnCheckForUpgradesSetIcons();
        PlayerBattleUIDelegates.InvokeOnResetAllAmmoSlotsCanvasGroupAlpha();
    }


    private void RestoreOriginalColor() {
        GetComponent<Image>().color = _originalColor;
    }
}