using System;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AmmoDragSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, Self] private RectTransform rectTransform;
    [SerializeField] Image ammoIcon;
    [field: SerializeField] public bool IsUpperCannon { get; private set; }

    private AmmoData _loadedAmmo = null;
    private Color _originalColor;
    
    void OnValidate() {
        this.ValidateRefs();
    }

    void OnEnable() {
        SetLoadedAmmo(_loadedAmmo);
    }

    void Start() {
        _originalColor = GetComponent<Image>().color;
    }

    public void OnDrop(PointerEventData eventData) {
        if (_loadedAmmo != null) return;
        GameObject draggedAmmo = eventData.pointerDrag;
        AmmoDragAndDropHandler dragDropHandler = draggedAmmo.GetComponent<AmmoDragAndDropHandler>();
        if (draggedAmmo == null) return;
        if (dragDropHandler == null) return;

        print("Ammo dropped onto slot");
        dragDropHandler.SetSuccessfulDrop(true);
        
        // AmmoData ammoData = dragDropHandler.AmmoData;
        // SetLoadedAmmo(ammoData);
        
        // TankDelegates.InvokeOnProjectileFire(ammoData, IsUpperCannon, 0);
        
    }

    private void SetLoadedAmmo(AmmoData ammoData) {
        if (ammoData == null) return;
        _loadedAmmo = ammoData;
        ammoIcon.sprite = ammoData.Icon;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        print("Pointer enter " + (IsUpperCannon ? "Upper Cannon" : "Lower Cannon"));
        GetComponent<Image>().color = Color.white;
    }

    public void OnPointerExit(PointerEventData eventData) {
        print("Pointer exit" + (IsUpperCannon ? "Upper Cannon" : "Lower Cannon"));
        GetComponent<Image>().color = _originalColor;
    }
}
