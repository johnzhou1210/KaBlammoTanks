using System;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AmmoDragAndDropHandler : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField, Parent] private Canvas canvas;
    [SerializeField, Self] private RectTransform rectTransform;
    [SerializeField, Self] private CanvasGroup canvasGroup;
    [SerializeField, Self] private Image image;

    [field: SerializeField] public AmmoData AmmoData { get; private set; }

    Vector2 _originalAnchoredPosition;
    bool _isSuccessfulDrop = false;
    
    void OnValidate() {
        this.ValidateRefs();
    }

    void OnEnable() {
        SetAmmoData(AmmoData);
    }

    public void OnPointerDown(PointerEventData eventData) {
        print("OnPointerDown");
    }

    public void OnBeginDrag(PointerEventData eventData) {
        
        print("OnBeginDrag");
        _originalAnchoredPosition = rectTransform.anchoredPosition;
        _isSuccessfulDrop = false;
        SetInteractable(false);
    }

    public void OnDrag(PointerEventData eventData) {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData) {
        print("OnEndDrag");
        rectTransform.anchoredPosition = _originalAnchoredPosition;
        SetInteractable(true);
        // SetInteractable(!_isSuccessfulDrop);
    }

    public void SetSuccessfulDrop(bool isSuccessfulDrop) {
        _isSuccessfulDrop = isSuccessfulDrop;
    }

    private void SetInteractable(bool isInteractable) {
        canvasGroup.blocksRaycasts = isInteractable;
        canvasGroup.alpha = isInteractable ? 1 : .6f;
        canvasGroup.interactable = isInteractable;
    }

    private void SetAmmoData(AmmoData ammoData = null) {
        if (ammoData == null) {
            ammoData = null;
            image.sprite = Resources.Load<Sprite>("AMMO_BLANK");
            return;
        }
        AmmoData = ammoData;
        image.sprite = ammoData.Icon;
    }
    
}
