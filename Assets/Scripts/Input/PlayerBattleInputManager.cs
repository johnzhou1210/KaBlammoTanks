using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerBattleInputManager : MonoBehaviour {
    private AmmoSlot _activeAmmoShopItem;
    [SerializeField] private Transform AmmoSlotContainer;
    void OnEnable() {
        PlayerBattleInputDelegates.OnShopAmmoTap += SetActiveAmmoShopItem;

        PlayerBattleInputDelegates.GetSelectedAmmoShopItem = () => _activeAmmoShopItem;
    }
    void OnDisable() {
        PlayerBattleInputDelegates.OnShopAmmoTap -= SetActiveAmmoShopItem;
        
        PlayerBattleInputDelegates.GetSelectedAmmoShopItem = null;
    }

    void Start() {
        // Get all ammo
        AmmoData[] allAmmo = Resources.LoadAll<AmmoData>("ScriptableObjects/Projectiles");
        // Assign all slots random ammo
        foreach (AmmoSlot ammoSlot in GetAllAmmoSlots()) {
            ammoSlot.SetSlotData(allAmmo[Random.Range(0, allAmmo.Length)]);
        }
        CheckForUpgrades();
    }
    private void SetActiveAmmoShopItem(AmmoSlot ammoSlot) {
        _activeAmmoShopItem = ammoSlot;
        AmmoData newAmmoData = ammoSlot.AmmoData;
        AnimateSelectionChanges();
        print("Active ammo shop item set to " + newAmmoData.AmmoName);
        PlayerBattleUIDelegates.InvokeOnShopItemDescriptionChanged(new(newAmmoData.AmmoName, newAmmoData.Damage.ToString()));
    }
    private void AnimateSelectionChanges() {
        List<AmmoSlot> allShopItems = GetAllAmmoSlots();
        foreach (AmmoSlot shopItem in allShopItems) {
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
    
    private List<AmmoSlot> GetAllAmmoSlots() {
        List<AmmoSlot> allShopItems = new List<AmmoSlot>();
        foreach (Transform child in AmmoSlotContainer) {
            allShopItems.Add(child.GetComponent<AmmoSlot>());
        }
        return allShopItems;
    }

    private void CheckForUpgrades() {
        // Record all desired upgradeWith items
        Dictionary<AmmoData,int> bagOfFreqs = new();
        foreach (AmmoSlot ammoSlot in GetAllAmmoSlots()) {
            AmmoData currAmmoData = ammoSlot.AmmoData;
            if (currAmmoData == null) {
                Debug.LogError("Ammo slot has no ammo data!!");
                continue;
            }
            if (currAmmoData.UpgradeRecipe.CombineWith != null) {
                AmmoData entryInQuestion = currAmmoData.UpgradeRecipe.CombineWith;
                if (bagOfFreqs.ContainsKey(entryInQuestion)) {
                    bagOfFreqs[entryInQuestion]++;
                } else {
                    bagOfFreqs.Add(entryInQuestion, 1);
                }
            }
        }
        // Label all slots if they exist in the set
        foreach (AmmoSlot ammoSlot in GetAllAmmoSlots()) {
            ammoSlot.SetUpgradeIconVisibility(bagOfFreqs.ContainsKey(ammoSlot.AmmoData) && bagOfFreqs[ammoSlot.AmmoData] > 1);
        }
    }
}
