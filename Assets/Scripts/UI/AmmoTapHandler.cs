using System;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AmmoTapHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public AmmoData AmmoData { get; private set; }
    [SerializeField] Image ammoIcon;
    [SerializeField, Child] TextMeshProUGUI costText;

    private bool _pointerInside = true; // Track if pointer is still inside

    void OnValidate() {
        this.ValidateRefs();
    }

    void OnEnable() {
        SetSlotData(AmmoData);
    }

    public void OnPointerDown(PointerEventData eventData) {
        print("Item pointer down");
        _pointerInside = true; // Assume pointer is down inside
    }

    public void OnPointerExit(PointerEventData eventData) {
        print("Pointer exited");
        _pointerInside = false; // Mark as exited
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (_pointerInside) {
            print("Item pointer up inside");
            PlayerBattleInputDelegates.InvokeOnShopAmmoTap(this);
        }
        else {
            print("Item pointer up outside");
        }
    }

    public void SetSlotData(AmmoData ammoData) {
        AmmoData = ammoData;
        ammoIcon.sprite = ammoData.Icon;
        costText.text = ammoData.Cost.ToString();
    }
}
