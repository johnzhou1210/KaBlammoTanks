using System;
using DG.Tweening;
using KBCore.Refs;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TankBillboardRender : MonoBehaviour {
    [SerializeField] TextMeshProUGUI playerHealthText, enemyHealthText, playerFractionLine, enemyFractionLine;
    [SerializeField] Image playerHealthFill, enemyHealthFill;


    void OnEnable() {
        // TankDelegates.OnUpdateTankHealthUI += UpdateHealth;

        PlayerBattleUIDelegates.GetHealthNumberUIPosition = GetHealthNumberUIPosition;
    }

    void OnDisable() {
        // TankDelegates.OnUpdateTankHealthUI -= UpdateHealth;
    }

    private void UpdateHealth(ulong id, int damage) {
        Image targetFill = id == 0 ? playerHealthFill : enemyHealthFill;
        TextMeshProUGUI targetFractionLine = id == 0 ? playerFractionLine : enemyFractionLine;
        TextMeshProUGUI targetText = id == 0 ? playerHealthText : enemyHealthText;
        
        TankController hostTankController = TankDelegates.GetHostTankController?.Invoke();
        TankController hosteeTankController = TankDelegates.GetHosteeTankController?.Invoke();

        if (hostTankController == null || hosteeTankController == null) {
            throw new Exception("Host or Hostee Tank Controller is null");
        }

        int currHealth, maxHealth;
        if (NetworkManager.Singleton.IsHost) {
            currHealth = hostTankController.TankHealth;
            maxHealth = hostTankController.TankMaxHealth;
        } else {
            currHealth = hosteeTankController.TankHealth;
            maxHealth = hosteeTankController.TankMaxHealth;
        }

        float targetFillAmount = (float)currHealth / maxHealth;

        DOTween.To(() => targetFill.fillAmount, x => targetFill.fillAmount = x, targetFillAmount, 0.5f).SetEase(Ease.OutCubic);

        int prevHealth = 0;
        if (int.TryParse(targetText.text.Split('\n')[0], out var parsed)) {
            prevHealth = parsed;
        }

        DOTween.To(() => prevHealth, x => {
            prevHealth = x;
            targetText.text = $"{x}\n{maxHealth}";
        }, currHealth, 0.5f).SetEase(Ease.OutCubic);
        
        Color low = new Color(1f, 0.1f, 0.1f);  // red-ish
        Color mid = new Color(1f, 1f, 0.1f);    // yellow-ish
        Color white = Color.white;

        Color targetColor;

        if (targetFillAmount >= 0.5f) {
            targetColor = white;
        } else {
            float t = targetFillAmount / 0.5f;  // scales 0–0.5 → 0–1
            targetColor = Color.Lerp(low, mid, t);
        }
        
        targetText.DOColor(targetColor, 0.5f).SetEase(Ease.OutCubic);
        targetFractionLine.DOColor(targetColor, 0.5f).SetEase(Ease.OutCubic);

    }

    private Vector3 GetHealthNumberUIPosition(ulong tankId) {
        return (tankId == 1 ? enemyFractionLine : playerFractionLine).transform.position;
    }
    
}
