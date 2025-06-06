using System;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AmmoSlot))]
[RequireComponent(typeof(AmmoUpgradeCombinerHandler))]
public class AmmoDragAndDropHandler : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [SerializeField, Self] AmmoSlot ammoSlot;
    [SerializeField, Parent] private Canvas canvas;
    [SerializeField] private RectTransform cardItemRectTransform;
    [SerializeField, Self] private CanvasGroup canvasGroup;
    [SerializeField] private LayoutElement layoutElement;
    private RectTransform _dragLayer;
    private Transform _originalParent;
    private int _originalSiblingIndex;
    CanvasGroup _ammoFrameCanvasGroup;
    void OnEnable() {
        _ammoFrameCanvasGroup = transform.parent.parent.GetComponent<CanvasGroup>();
        _dragLayer = PlayerBattleUIDelegates.GetDragLayerRectTransform?.Invoke();
    }
    void OnValidate() {
        this.ValidateRefs(); // Optional: use if you have KBCore Refs. If not, remove.
    }

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnBeginDrag(PointerEventData eventData) {
        
        SetInteractable(false);
        layoutElement.ignoreLayout = true;
        _originalParent = cardItemRectTransform.parent;
        _originalSiblingIndex = cardItemRectTransform.GetSiblingIndex();

        // Move to drag layer
        cardItemRectTransform.SetParent(_dragLayer, true);

        // Reset anchors/pivot to center
        cardItemRectTransform.anchorMin = cardItemRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        cardItemRectTransform.pivot = new Vector2(0.5f, 0.5f);

        // Position under mouse
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragLayer, eventData.position, eventData.pressEventCamera, out Vector2 localMousePos);
        cardItemRectTransform.anchoredPosition = localMousePos;

        // Show drop indicator
        PlayerBattleUIDelegates.InvokeOnSetDropIndicatorActive(true);
        PlayerBattleUIDelegates.InvokeOnDropIndicatorSetParent(_originalParent, false);
        UpdateDropIndicatorPosition(eventData);

  
    }
    public void OnDrag(PointerEventData eventData) {
        EventSystem.current.SetSelectedGameObject(gameObject);
        
        // Move the object freely under mouse
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragLayer, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        cardItemRectTransform.anchoredPosition = localPoint;

        // Find where to reorder in the original layout group
        int newIndex = FindClosestSiblingIndex(eventData);
        if (newIndex != _originalSiblingIndex) {
            _originalSiblingIndex = newIndex;
        }
        UpdateDropIndicatorPosition(eventData);

    
    }
    public void OnEndDrag(PointerEventData eventData) {
   
        
        SetInteractable(true);
        layoutElement.ignoreLayout = false;

        // Reparent back to original layout group
        cardItemRectTransform.SetParent(_originalParent, true);
        cardItemRectTransform.SetSiblingIndex(_originalSiblingIndex);
        PlayerBattleUIDelegates.InvokeOnSetDropIndicatorActive(false);
        
        // Check for any automatic upgrades
        PlayerBattleUIDelegates.InvokeOnDoAutoUpgrades();
        

    }
    private void SetInteractable(bool isInteractable) {
        // canvasGroup.blocksRaycasts = isInteractable;
        _ammoFrameCanvasGroup.alpha = isInteractable ? 1 : .6f;
        _ammoFrameCanvasGroup.interactable = isInteractable;
        _ammoFrameCanvasGroup.blocksRaycasts = isInteractable;
        // canvasGroup.interactable = isInteractable;

        // Get ammo data of dragged item
        AmmoData draggedAmmoData = ammoSlot.AmmoData;

        // If the ammo is upgradeable, make the ammo that can be combined with opaque
        foreach (Transform child in _ammoFrameCanvasGroup.transform) {
            AmmoSlot currAmmoSlotScript = child.GetComponentInChildren<AmmoSlot>();
            if (currAmmoSlotScript == null) {
                continue;
            }
            AmmoData currAmmoData = currAmmoSlotScript.AmmoData;
            if (currAmmoData == null) {
                Debug.LogError("An ammo slot does not have any ammo data!!");
                continue;
            }
            child.GetComponentInChildren<CanvasGroup>().alpha = isInteractable || child == transform ? 1 :
                draggedAmmoData.UpgradeRecipe.CombineWith == currAmmoData ? 1 : .6f;
        }
    }
    private int FindClosestSiblingIndex(PointerEventData eventData) {
        
        int closestIndex = CountAmmoSlotChildren(_originalParent);
        float closestDistance = float.MaxValue;
        for (int i = 0; i < CountAmmoSlotChildren(_originalParent) + 1; i++) {
            Transform sibling = _originalParent.GetChild(i);
            RectTransform siblingRect = sibling as RectTransform;
            Vector2 siblingScreenPos = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, siblingRect.position);
            float distance = eventData.position.x - siblingScreenPos.x;
            if (Mathf.Abs(distance) < closestDistance) {
                closestDistance = Mathf.Abs(distance);
                closestIndex = i;
            }
        }
        return closestIndex;
    }
    private void UpdateDropIndicatorPosition(PointerEventData eventData) {
        int newIndex = FindClosestSiblingIndex(eventData);

        // Move dropIndicator to the calculated index
        newIndex = Mathf.Clamp(newIndex, 0, CountAmmoSlotChildren(_originalParent));
        PlayerBattleUIDelegates.InvokeOnSetDropIndicatorSiblingIndex(newIndex);
    }
    private int CountAmmoSlotChildren(Transform ammoSlotContainerTransform) {
        int result = 0;
        foreach (Transform child in ammoSlotContainerTransform) {
            if (child.GetComponentInChildren<AmmoSlot>() != null)
                result++;
        }
        return result;
    }
}
