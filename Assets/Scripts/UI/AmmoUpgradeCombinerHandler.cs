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
    }

    private void OnDisable() {
        RestoreOriginalColor();
    }

    private void OnValidate() {
        this.ValidateRefs();
    }

    public float GetPitchBasedOnResultRarity(Rarity rarity) {
        switch (rarity) {
            case Rarity.COMMON: return .8f;
            case Rarity.RARE: return .9f;
            case Rarity.EPIC: return 1f;
            case Rarity.LEGENDARY: return 1.1f;
            default: return 0f;
        }
    }


    private List<AmmoSlot> GetAllAmmoSlots() {
        return PlayerBattleInputDelegates.GetAllAmmoSlots?.Invoke();
    }


    // private IEnumerator AutoUpgradeCoroutine() {
    //     while (CanAutoUpgrade()) {
    //         Debug.Log("Combining ammo");
    //         yield return null;
    //     }
    // }


    private void RestoreOriginalColor() {
        GetComponent<Image>().color = _originalColor;
    }
}