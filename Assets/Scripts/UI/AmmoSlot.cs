using System;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class AmmoSlot : MonoBehaviour
{
    public AmmoData AmmoData { get; private set; }
    [SerializeField] Image ammoIcon;
    [SerializeField, Child] TextMeshProUGUI costText;
    [SerializeField] private GameObject upgradeAvailableIcon;

    private void OnValidate() {
        this.ValidateRefs();
    }
    
    public void SetSlotData(AmmoData ammoData) { ;
        if (ammoData == null) return;
        AmmoData = ammoData;
        ammoIcon.sprite = ammoData.Icon;
        costText.text = ammoData.Cost.ToString();
    }
    
    public void SetUpgradeIconVisibility(bool val) {
        upgradeAvailableIcon.SetActive(val);
    }
}
