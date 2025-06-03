using System;
using KBCore.Refs;
using UnityEngine;

public class DragLayer : MonoBehaviour
{
    [SerializeField, Self] private RectTransform rectTransform;

    private void OnEnable() {
        PlayerBattleUIDelegates.GetDragLayerRectTransform = () => rectTransform;
    }

    private void OnDisable() {
        PlayerBattleUIDelegates.GetDragLayerRectTransform = null;
    }

    private void OnValidate() {
        this.ValidateRefs();
    }
}
