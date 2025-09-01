using TMPro;
using UnityEngine;

public class TankDisplay : MonoBehaviour {
    [SerializeField] private GameObject identityMarker, triangle;
    [SerializeField] private TextMeshPro identityText;

    public void SetIdentityMarker(string text, Color color) {
        identityText.SetText(text);
        identityText.color = color;
        triangle.GetComponent<SpriteRenderer>().color = color;
    }
}
