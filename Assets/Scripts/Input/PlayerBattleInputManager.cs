using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBattleInputManager : MonoBehaviour {
    AmmoTapHandler _activeAmmoShopItem;
    [SerializeField] Transform AmmoTapHandlerContainer;
    void OnEnable() {
        PlayerBattleInputDelegates.OnShopAmmoTap += SetActiveAmmoShopItem;

        PlayerBattleInputDelegates.GetSelectedAmmoShopItem = () => _activeAmmoShopItem;
    }
    void OnDisable() {
        PlayerBattleInputDelegates.OnShopAmmoTap -= SetActiveAmmoShopItem;
        
        PlayerBattleInputDelegates.GetSelectedAmmoShopItem = null;
    }
    private void SetActiveAmmoShopItem(AmmoTapHandler ammoTapHandler) {
        _activeAmmoShopItem = ammoTapHandler;
        AmmoData newAmmoData = ammoTapHandler.AmmoData;
        AnimateSelectionChanges();
        print("Active ammo shop item set to " + newAmmoData.AmmoName);
        PlayerBattleUIDelegates.InvokeOnShopItemDescriptionChanged(new(newAmmoData.AmmoName, newAmmoData.Damage.ToString()));
    }
    private void AnimateSelectionChanges() {
        List<AmmoTapHandler> allShopItems = GetAllAmmoTapHandlers();
        foreach (AmmoTapHandler shopItem in allShopItems) {
            Animator currAnimator = shopItem.GetComponent<Animator>();
            if (shopItem != _activeAmmoShopItem) {
                // Do nothing, except if the item's animator is still playing selected animation
                if (GetIsSelectionAnimationPlaying(currAnimator)) {
                    currAnimator.Play("AmmoShopUIDeselect");
                }
            } else {
                // Play select animation if ui is not already playing select animation
                if (!GetIsSelectionAnimationPlaying(currAnimator)) {
                    currAnimator.Play("AmmoShopUISelect");
                }
            }
        }
    }

    private bool GetIsSelectionAnimationPlaying(Animator animator) {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("AmmoShopUISelected") || animator.GetCurrentAnimatorStateInfo(0).IsName("AmmoShopUISelect");
    }
    
    private List<AmmoTapHandler> GetAllAmmoTapHandlers() {
        List<AmmoTapHandler> allShopItems = new List<AmmoTapHandler>();
        foreach (Transform child in AmmoTapHandlerContainer) {
            allShopItems.Add(child.GetComponent<AmmoTapHandler>());
        }
        return allShopItems;
    }
}
