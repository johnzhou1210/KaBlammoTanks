using System;
using KBCore.Refs;
using TMPro;
using UnityEngine;

public class DamageIndicator : MonoBehaviour {
    private int _damageNumber = 0;
    [SerializeField, Self] private TextMeshProUGUI _damageNumberText;

    private void OnValidate() {
        this.ValidateRefs();
    }

    public void SetDamageNumber(int damageNumber) {
        _damageNumber = damageNumber;
        _damageNumberText.text = _damageNumber.ToString();
    }
    
    
}
