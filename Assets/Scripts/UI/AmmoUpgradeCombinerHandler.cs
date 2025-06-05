using System;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AmmoSlot))]
public class AmmoUpgradeCombinerHandler : MonoBehaviour {
    [SerializeField, Self] private AmmoSlot ammoSlot;
    
    private Color _originalColor;

    private void OnValidate() {
        this.ValidateRefs();
    }

    private void OnEnable() {
        _originalColor = GetComponent<Image>().color;
    }

    private void OnDisable() {
     RestoreOriginalColor();   
    }
    
    public void OnDrop(PointerEventData eventData) {
        print("Ammo dropped onto slot");
        
        AmmoSlot droppedAmmoSlot = eventData.pointerDrag.GetComponent<AmmoSlot>();
        if (droppedAmmoSlot == null) {
            Debug.LogError("Dropped ui element has no ammo slot!");
            return;
        }
        
        // Attempt to combine
        // AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/SFX/AmmoCombine"), GetPitchBasedOnResultRarity(ammoSlot.AmmoData.Rarity));
        
        // Remove the dropped ammo slot UI element, and update THIS ammo slot (not the removed one)
        Destroy(droppedAmmoSlot.gameObject);
        ammoSlot.SetSlotData(ammoSlot.AmmoData.UpgradeRecipe.UpgradesTo);
        
        // Reassess upgrade available for all ammo
        PlayerBattleUIDelegates.InvokeOnCheckForUpgradesSetIcons();
        PlayerBattleUIDelegates.InvokeOnResetAllAmmoSlotsCanvasGroupAlpha();

        this.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        print("Pointer enter ");
        GetComponent<Image>().color = Color.white;
        
        // Disable ghost slot
        PlayerBattleUIDelegates.InvokeOnSetDropIndicatorActive(false);
    }

    public void OnPointerExit(PointerEventData eventData) {
        print("Pointer exit");
        RestoreOriginalColor();
        
        // Reenable ghost slot
        PlayerBattleUIDelegates.InvokeOnSetDropIndicatorActive(true);
    }

    private void RestoreOriginalColor() {
        GetComponent<Image>().color = _originalColor;
    }

   
    
}
