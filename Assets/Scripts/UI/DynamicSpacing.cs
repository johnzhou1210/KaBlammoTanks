using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HorizontalLayoutGroup))]
public class DynamicSpacing : MonoBehaviour {
    private HorizontalLayoutGroup _layoutGroup;

    [SerializeField] private int minChildren = 2;
    [SerializeField] private int maxChildren = 8;
    [SerializeField] private float spacingAtMin = -680f;
    [SerializeField] private float spacingAtMax = -64f;

    private void Awake() {
        _layoutGroup = GetComponent<HorizontalLayoutGroup>();
    }

    private void LateUpdate() {
        int activeChildren = 0;
        foreach (Transform child in transform)
            if (child.gameObject.activeSelf)
                activeChildren++;

        if (activeChildren <= 0) return;

        int clampedCount = Mathf.Clamp(activeChildren, minChildren, maxChildren);

        // Normalize between 0 and 1
        float t = (clampedCount - minChildren) / (float)(maxChildren - minChildren);

        // Apply sqrt curve (fast early, slow later)
        float curvedT = Mathf.Pow(t, 0.75f);

        _layoutGroup.spacing = Mathf.Lerp(spacingAtMin, spacingAtMax, curvedT);
    }
}
