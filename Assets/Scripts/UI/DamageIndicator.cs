using System;
using DG.Tweening;
using KBCore.Refs;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class DamageIndicator : MonoBehaviour {
    private int _damageNumber = 0;
    [SerializeField, Self] private TextMeshProUGUI damageNumberText;
    [SerializeField] private CanvasGroup canvasGroup;
    private void Start() {
        canvasGroup = transform.parent.GetComponent<CanvasGroup>();
    }
    public void Initialize(int damageNumber, ulong tankId) {
        _damageNumber = damageNumber;
        damageNumberText.text = _damageNumber.ToString();
        Vector3 startPos = transform.position;
        Vector3 floatUpPos = startPos + Vector3.up * 50f; // Adjust upward offset as needed
        Debug.Log($"Id of target pos : {tankId}");
        Vector3 targetPos = PlayerBattleUIDelegates.GetHealthNumberUIPosition(tankId);
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMove(floatUpPos, 0.3f).SetEase(Ease.OutQuad));
        seq.Append(transform.DOMove(targetPos, 0.4f).SetEase(Ease.InQuad));
        seq.Join(canvasGroup.DOFade(0f, 0.4f));
        seq.AppendCallback(() => {
            Debug.Log("AppendCallback fired!");
            if (NetworkManager.Singleton.IsServer) {
                Debug.Log("Calling RPC...");
                TakeDamageServerRpc(damageNumber, tankId);
            }
        });
        seq.AppendCallback(() => Destroy(gameObject));
    }
    [ServerRpc]
    private void TakeDamageServerRpc(int damage, ulong targetId, ServerRpcParams serverRpcParams = default) {
        Debug.Log("IN TAKEDAMAGESERVERRPC");
        Debug.Log($"[ServerRpc] Sending damage={damage} to targetId={targetId}");
        TankDelegates.InvokeOnTakeDamage(_damageNumber, targetId);
    }
}
