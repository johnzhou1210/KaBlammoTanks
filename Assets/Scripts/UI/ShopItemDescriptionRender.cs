using System;
using KBCore.Refs;
using TMPro;
using UnityEngine;

public struct TitleDamagePair {
    public String Title;
    public String Description;
    public TitleDamagePair(string title, string description) {
        Title = title;
        Description = description;
    }
}

public class ShopItemDescriptionRender : MonoBehaviour {
    TitleDamagePair _currentTitleDamagePair;
    [SerializeField] TextMeshProUGUI descriptionTitle, descriptionDescription;
    
    void OnEnable() {
        PlayerBattleUIDelegates.OnDescriptionDataChanged += UpdateDescription;
    }

    void OnDisable() {
        PlayerBattleUIDelegates.OnDescriptionDataChanged -= UpdateDescription;
    }

    private void UpdateDescription(TitleDamagePair pair) {
        _currentTitleDamagePair = pair;
        RenderDescription();
    }

    private void RenderDescription() {
        descriptionTitle.text = _currentTitleDamagePair.Title;
        descriptionDescription.text = _currentTitleDamagePair.Description;
    }
}
