using System;
using Coffee.UIEffects;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class AmmoSlot : MonoBehaviour
{
    public AmmoData AmmoData { get; private set; }
    [SerializeField] Image ammoIcon;
    [SerializeField] private TextMeshProUGUI ammoName, damageText;
    [SerializeField] private GameObject upgradeAvailableIcon;
    [SerializeField] private UIEffect AmmoSlotUIEffect, MainFrameUIEffect;
    
    public void SetSlotData(AmmoData ammoData) { ;
        if (ammoData == null) return;
        AmmoData = ammoData;
        ammoIcon.sprite = ammoData.Icon;
        ammoName.text = ammoData.AmmoName;
        damageText.text = ammoData.Damage.ToString();
        LoadUIEffectBasedOnRarity();
    }
    
    public void SetUpgradeIconVisibility(bool val) {
        upgradeAvailableIcon.SetActive(val);
    }

    private void LoadUIEffectBasedOnRarity() {
        if (AmmoData.Rarity != Rarity.LEGENDARY) {
            AmmoSlotUIEffect.LoadPreset("NonLegendaryRarityAmmoSlot");
        } else {
            AmmoSlotUIEffect.LoadPreset("LegendaryRarityAmmoSlot");
        }
        switch (AmmoData.Rarity) {
            case Rarity.COMMON:
                MainFrameUIEffect.LoadPreset("CommonRarityMainFrame");
                break;
            case Rarity.RARE:
                MainFrameUIEffect.LoadPreset("RareRarityMainFrame");
                break;
            case Rarity.EPIC:
                MainFrameUIEffect.LoadPreset("EpicRarityMainFrame");
                break;
            case Rarity.LEGENDARY:
                MainFrameUIEffect.LoadPreset("LegendaryRarityMainFrame");
                break;
            default:
                Debug.LogError("Unknown Rarity!");
                break;
        }
        Debug.Log("Set frame to stuff of rarity " + AmmoData.Rarity);
    }
}
