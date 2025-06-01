using System;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AmmoTapHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public AmmoData AmmoData;
    [SerializeField] Image ammoIcon;
    [SerializeField, Child] TextMeshProUGUI costText;
    
    void OnValidate() {
        this.ValidateRefs();
    }

    void OnEnable() {
        SetSlotData(AmmoData);
    }

    public void OnPointerDown(PointerEventData eventData) {
        print("Item pointer down");
    }

    public void OnPointerUp(PointerEventData eventData) {
        print("Item pointer up");
        PlayerBattleInputDelegates.InvokeOnShopAmmoTap(this);
    }

    private void SetSlotData(AmmoData ammoData) {
        ammoIcon.sprite = ammoData.Icon;
        costText.text = ammoData.Cost.ToString();
    }
}
