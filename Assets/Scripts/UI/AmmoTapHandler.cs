using KBCore.Refs;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AmmoSlot))]
public class AmmoTapHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler {
    [SerializeField] [Self] private AmmoSlot ammoSlot;
    private float _pointerDownTime;

    private bool _pointerInside = true; // Track if pointer is still inside
    private readonly float _tapWindowThreshold = 0.5f;

    private void OnEnable() {
        ammoSlot.SetSlotData(ammoSlot.AmmoData);
    }

    private void OnValidate() {
        this.ValidateRefs();
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (!ammoSlot.IsInteractable()) return;

        // if (!DragLock.TryStartDrag(eventData.pointerId)) return;
        // print("Item pointer down");
        _pointerInside = true; // Assume pointer is down inside
        _pointerDownTime = Time.time;
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!ammoSlot.IsInteractable()) return;

        // print("Pointer exited");
        _pointerInside = false; // Mark as exited
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (!ammoSlot.IsInteractable()) return;

        if (_pointerInside) {
            float pointerUpTime = Time.time;
            float heldDuration = pointerUpTime - _pointerDownTime;

            if (_pointerInside && heldDuration <= _tapWindowThreshold) {
                // print("Item pointer up inside, registered");
                AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/SFX/PopSound"), 1f);
                PlayerBattleInputDelegates.InvokeOnShopAmmoTap(ammoSlot);
            } else {
                // print("Item pointer up inside, but cancelled");
            }
        } else {
            // print("Item pointer up outside");
        }
    }
}