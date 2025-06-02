using System;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AmmoSlot))]
public class AmmoTapHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler {
    [SerializeField, Self] private AmmoSlot ammoSlot;

    private bool _pointerInside = true; // Track if pointer is still inside

    void OnValidate() {
        this.ValidateRefs();
    }

    void OnEnable() {
        ammoSlot.SetSlotData(ammoSlot.AmmoData);
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
            PlayerBattleInputDelegates.InvokeOnShopAmmoTap(ammoSlot);
        }
        else {
            print("Item pointer up outside");
        }
    }

    
}
