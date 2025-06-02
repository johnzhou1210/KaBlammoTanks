using System;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AmmoDragAndDropHandler : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField, Parent] private Canvas canvas; // Main canvas
    [SerializeField, Self] private RectTransform rectTransform;
    [SerializeField, Self] private CanvasGroup canvasGroup;
    [SerializeField, Self] private LayoutElement layoutElement;
    [SerializeField] private RectTransform dragLayer; // <-- Drag Layer in Canvas
    private Transform originalParent;
    private int originalSiblingIndex;
    private bool _isSuccessfulDrop = false;

    void OnValidate() {
        this.ValidateRefs(); // Optional: use if you have KBCore Refs. If not, remove.
    }

    public void OnPointerDown(PointerEventData eventData) {
        _isSuccessfulDrop = false;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        SetInteractable(false);
        layoutElement.ignoreLayout = true; // Disable layout control temporarily

        originalParent = rectTransform.parent;
        originalSiblingIndex = rectTransform.GetSiblingIndex();

        // Move to drag layer
        rectTransform.SetParent(dragLayer, true);

        // Reset anchors/pivot to center
        rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // Position under mouse
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragLayer,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localMousePos
        );
        rectTransform.anchoredPosition = localMousePos;
    }

    public void OnDrag(PointerEventData eventData) {
        // Move the object freely under mouse
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragLayer,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );
        rectTransform.anchoredPosition = localPoint;

        // Find where to reorder in the original layout group
        int newIndex = FindClosestSiblingIndex(eventData);

        if (newIndex != originalSiblingIndex) {
            originalSiblingIndex = newIndex;
        }
    }

    public void OnEndDrag(PointerEventData eventData) {
        SetInteractable(true);
        layoutElement.ignoreLayout = false;

        // Reparent back to original layout group
        rectTransform.SetParent(originalParent, true);
        rectTransform.SetSiblingIndex(originalSiblingIndex);
    }

    public void SetSuccessfulDrop(bool isSuccessfulDrop) {
        _isSuccessfulDrop = isSuccessfulDrop;
    }

    private void SetInteractable(bool isInteractable) {
        canvasGroup.blocksRaycasts = isInteractable;
        canvasGroup.alpha = isInteractable ? 1 : .6f;
        canvasGroup.interactable = isInteractable;
    }

    private int FindClosestSiblingIndex(PointerEventData eventData) {
        int closestIndex = originalParent.childCount;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < originalParent.childCount; i++) {
            Transform sibling = originalParent.GetChild(i);

            RectTransform siblingRect = sibling as RectTransform;

            Vector2 siblingScreenPos = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, siblingRect.position);

            float distance = eventData.position.x - siblingScreenPos.x;

            if (distance < 0 && Mathf.Abs(distance) < closestDistance) {
                closestDistance = Mathf.Abs(distance);
                closestIndex = i;
            }
        }

        return closestIndex;
    }
}
