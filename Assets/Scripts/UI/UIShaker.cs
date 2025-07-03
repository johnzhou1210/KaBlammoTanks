using DG.Tweening;
using UnityEngine;

public class UIShaker : MonoBehaviour {
    public RectTransform targetUI;

    public void DoShake() {
        Shake(4f,10f);
    }

    public void Shake(float duration = 0.2f, float strength = 10f) {
        if (targetUI != null) {
            targetUI.DOShakeAnchorPos(duration, strength, vibrato: 10, randomness: 90, snapping: false, fadeOut: true);
        }
    }
}
