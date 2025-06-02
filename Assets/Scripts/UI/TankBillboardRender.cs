using System;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TankBillboardRender : MonoBehaviour {
    [SerializeField] TextMeshProUGUI playerHealthText, enemyHealthText;
    [SerializeField] Slider playerHealthSlider, enemyHealthSlider;


    void OnEnable() {
        TankDelegates.OnUpdateTankHealthUI += UpdateHealth;
    }

    void OnDisable() {
        TankDelegates.OnUpdateTankHealthUI -= UpdateHealth;
    }

    private void UpdateHealth(int id, int damage) {
        Slider targetSlider = id == 0 ? playerHealthSlider : enemyHealthSlider;
        TextMeshProUGUI targetText = id == 0 ? playerHealthText : enemyHealthText;
        
        int currHealth = TankDelegates.GetTankHealthById?.Invoke(id) ?? 0;
        int maxHealth = TankDelegates.GetTankMaxHealthById?.Invoke(id) ?? 0;
        targetSlider.value = (float)currHealth / maxHealth;
        targetText.text = $"{currHealth} / {maxHealth}";
        
    }
    
}
