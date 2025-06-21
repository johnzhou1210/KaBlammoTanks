using System;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TankBillboardRender : MonoBehaviour {
    [SerializeField] TextMeshProUGUI playerHealthText, enemyHealthText, playerFractionLine, enemyFractionLine;
    [SerializeField] Image playerHealthFill, enemyHealthFill;


    void OnEnable() {
        TankDelegates.OnUpdateTankHealthUI += UpdateHealth;
    }

    void OnDisable() {
        TankDelegates.OnUpdateTankHealthUI -= UpdateHealth;
    }

    private void UpdateHealth(int id, int damage) {
        Image targetFill = id == 0 ? playerHealthFill : enemyHealthFill;
        TextMeshProUGUI targetFractionLine = id == 0 ? playerFractionLine : enemyFractionLine;
        TextMeshProUGUI targetText = id == 0 ? playerHealthText : enemyHealthText;
        
        int currHealth = TankDelegates.GetTankHealthById?.Invoke(id) ?? 0;
        int maxHealth = TankDelegates.GetTankMaxHealthById?.Invoke(id) ?? 0;
        targetFill.fillAmount = (float)currHealth / maxHealth;
        targetText.text = $"{currHealth}\n{maxHealth}";
        
    }
    
}
