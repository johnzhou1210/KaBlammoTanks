using System;
using System.Collections;
using DG.Tweening;
using KBCore.Refs;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TankBillboardRender : MonoBehaviour {
    [SerializeField] TextMeshProUGUI hostHealthText, hosteeHealthText, hostFractionLine, hosteeFractionLine;
    [SerializeField] Image hostHealthFill, hosteeHealthFill;


    void OnEnable() {
        TankDelegates.OnUpdateTankHealthUI += UpdateHealth;

        PlayerBattleUIDelegates.GetHealthNumberUIPosition = GetHealthNumberUIPosition;
    }

    void OnDisable() {
        TankDelegates.OnUpdateTankHealthUI -= UpdateHealth;
    }

    private void UpdateHealth(ulong id, int newHealth, int newMaxHealth) {
        Debug.Log($"UpdateHealth: {id}, {newHealth}, {newMaxHealth}");
        
        Image targetFill = id == 0 ? hostHealthFill : hosteeHealthFill;
        TextMeshProUGUI targetFractionLine = id == 0 ? hostFractionLine : hosteeFractionLine;
        TextMeshProUGUI targetText = id == 0 ? hostHealthText : hosteeHealthText;
        
        TankController hostTankController = TankDelegates.GetHostTankController?.Invoke();
        TankController hosteeTankController = TankDelegates.GetHosteeTankController?.Invoke();

        if (hostTankController == null || hosteeTankController == null) {
            throw new Exception("Host or Hostee Tank Controller is null");
        }
        
        float targetFillAmount = (float)newHealth / newMaxHealth;

        DOTween.To(() => targetFill.fillAmount, x => targetFill.fillAmount = x, targetFillAmount, 0.5f).SetEase(Ease.OutCubic).SetUpdate(true);

        int prevHealth = 0;
        Debug.Log(targetText);
        if (int.TryParse(targetText.text.Split('\n')[0], out var parsed)) {
            prevHealth = parsed;
        }

        DOTween.To(() => prevHealth, x => {
            prevHealth = x;
            targetText.text = $"{x}\n{newMaxHealth}";
        }, newHealth, 0.5f).SetEase(Ease.OutCubic).SetUpdate(true);
        
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
        
        targetText.DOColor(targetColor, 0.5f).SetEase(Ease.OutCubic).SetUpdate(true);
        targetFractionLine.DOColor(targetColor, 0.5f).SetEase(Ease.OutCubic).SetUpdate(true);
        
    }

    
  

    private Vector3 GetHealthNumberUIPosition(ulong tankId) {
        return (tankId == TankDelegates.GetHosteeId?.Invoke() ? hosteeFractionLine : hostFractionLine).transform.position;
    }
    
}
