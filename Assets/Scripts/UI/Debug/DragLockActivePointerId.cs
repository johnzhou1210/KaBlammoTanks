using System;
using KBCore.Refs;
using TMPro;
using UnityEngine;

public class DragLockActivePointerId : MonoBehaviour {
    [SerializeField, Self] private TextMeshProUGUI textMesh;
    private void OnValidate() {
        this.ValidateRefs();
    }

    private void Update() {
        // textMesh.text = "Active pointer id: " + DragLock.ActivePointerId.ToString();
    }
}
