using UnityEngine;
using UnityEngine.EventSystems;

public class AmmoSelectionCancelAreaHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private float _tapWindowThreshold = .5f;
    private float _pointerDownTime = 0f;
    
    
    public void OnPointerDown(PointerEventData eventData) {
        _pointerDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData) {
        float pointerUpTime = Time.time;
        float heldDuration = pointerUpTime - _pointerDownTime;
        if (heldDuration <= _tapWindowThreshold) {
            // Cancel active ammo selection if there is one
            AmmoSlot selectedSlot = PlayerBattleInputDelegates.GetSelectedAmmoShopItem?.Invoke();
            if (selectedSlot != null) {
                PlayerBattleInputDelegates.InvokeOnShopAmmoTap(null);
                AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/SFX/SelectionCancel"), 1f);
            }
        }
    }
}
