using System;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AmmoSlot))]
public class AmmoTapHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler {
    [SerializeField, Self] private AmmoSlot ammoSlot;
    private float _tapWindowThreshold = 0.5f;
    private float _pointerDownTime = 0f;

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
        _pointerDownTime = Time.time;
    }

    public void OnPointerExit(PointerEventData eventData) {
        print("Pointer exited");
        _pointerInside = false; // Mark as exited
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (_pointerInside) {
            float pointerUpTime = Time.time;
            float heldDuration = pointerUpTime - _pointerDownTime;

            if (_pointerInside && heldDuration <= _tapWindowThreshold) {
                print("Item pointer up inside, registered");
                AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/SFX/PopSound"), 1f);
                PlayerBattleInputDelegates.InvokeOnShopAmmoTap(ammoSlot);
            } else {
                print("Item pointer up inside, but cancelled");
            }
            
            
        }
        else {
            print("Item pointer up outside");
        }
    }


}
