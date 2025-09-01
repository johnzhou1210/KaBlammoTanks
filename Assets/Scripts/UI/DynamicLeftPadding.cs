using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HorizontalLayoutGroup))]
public class DynamicLeftPadding : MonoBehaviour
{
    public int minElements = 2;    // Elements for max padding
    public int maxElements = 8;    // Elements for min padding
    public int maxPadding = 200;   // Left padding for few elements
    public int minPadding = -500;  // Left padding for many elements

    private HorizontalLayoutGroup layoutGroup;

    void Awake()
    {
        layoutGroup = GetComponent<HorizontalLayoutGroup>();
    }

    void LateUpdate()
    {
        int activeChildren = 0;
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
                activeChildren++;
        }

        // Clamp active children count
        activeChildren = Mathf.Clamp(activeChildren, minElements, maxElements);

        // Linear interpolation from maxPadding to minPadding
        float t = (float)(activeChildren - minElements) / (maxElements - minElements);
        int dynamicPadding = Mathf.RoundToInt(Mathf.Lerp(maxPadding, minPadding, t));

        layoutGroup.padding.left = dynamicPadding;
        layoutGroup.SetLayoutHorizontal(); // Force update immediately
    }
}
