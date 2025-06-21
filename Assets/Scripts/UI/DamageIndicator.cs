using System;
using DG.Tweening;
using KBCore.Refs;
using TMPro;
using UnityEngine;

public class DamageIndicator : MonoBehaviour {
    private int _damageNumber = 0;
    [SerializeField, Self] private TextMeshProUGUI damageNumberText;
    [SerializeField, Parent] private CanvasGroup canvasGroup;

    private void OnValidate() {
        this.ValidateRefs();
    }

    public void Initialize(int damageNumber, int tankId) {
        _damageNumber = damageNumber;
        damageNumberText.text = _damageNumber.ToString();

        Vector3 startPos = transform.position;
        Vector3 floatUpPos = startPos + Vector3.up * 50f; // Adjust upward offset as needed
        Vector3 targetPos = PlayerBattleUIDelegates.GetHealthNumberUIPosition(tankId);

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMove(floatUpPos, 0.3f).SetEase(Ease.OutQuad));
        seq.Append(transform.DOMove(targetPos, 0.4f).SetEase(Ease.InQuad));
        seq.Join(canvasGroup.DOFade(0f, 0.4f));
        seq.AppendCallback(() => Destroy(gameObject));
    }

    

}
