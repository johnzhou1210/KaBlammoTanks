using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class PlayerBattleInputManager : MonoBehaviour {
    private AmmoSlot _activeAmmoShopItem;
    [SerializeField] private Transform AmmoSlotContainer;
    [SerializeField] private RectTransform DropIndicator;

    private Coroutine _supplyAmmoCoroutine, _autoUpgradeCoroutine;
    
    void OnEnable() {
        PlayerBattleInputDelegates.OnShopAmmoTap += SetActiveAmmoShopItem;
        PlayerBattleInputDelegates.OnRemoveActiveAmmoShopItem += RemoveActiveAmmoShopItem;
        PlayerBattleInputDelegates.OnDoAutoUpgrades += DoAutoUpgrades;
        
     
        PlayerBattleUIDelegates.OnCheckForUpgradesSetIcons += CheckForUpgradesPatient;
        PlayerBattleUIDelegates.OnResetAllAmmoSlotsCanvasGroupAlpha += ResetAllAmmoSlotsCanvasGroupAlpha;
        PlayerBattleUIDelegates.OnSetDropIndicatorSiblingIndex += SetDropIndicatorSiblingIndex;
        PlayerBattleUIDelegates.OnSetDropIndicatorActive += SetDropIndicatorActive;
        PlayerBattleUIDelegates.OnDropIndicatorSetParent += SetDropIndicatorParent;
       

        PlayerBattleInputDelegates.GetSelectedAmmoShopItem = () => _activeAmmoShopItem;
    }
    void OnDisable() {
        PlayerBattleInputDelegates.OnShopAmmoTap -= SetActiveAmmoShopItem;
        PlayerBattleInputDelegates.OnRemoveActiveAmmoShopItem -= RemoveActiveAmmoShopItem;
        PlayerBattleInputDelegates.OnDoAutoUpgrades -= DoAutoUpgrades;
        
        PlayerBattleUIDelegates.OnCheckForUpgradesSetIcons -= CheckForUpgradesPatient;
        PlayerBattleUIDelegates.OnResetAllAmmoSlotsCanvasGroupAlpha -= ResetAllAmmoSlotsCanvasGroupAlpha;
        PlayerBattleUIDelegates.OnSetDropIndicatorSiblingIndex -= SetDropIndicatorSiblingIndex;
        PlayerBattleUIDelegates.OnSetDropIndicatorActive -= SetDropIndicatorActive;
        PlayerBattleUIDelegates.OnDropIndicatorSetParent -= SetDropIndicatorParent;
        
        PlayerBattleInputDelegates.GetSelectedAmmoShopItem = null;
    }

    void Start() {
        EventSystem.current.pixelDragThreshold = 15;
        StartCoroutine(InitializeInput());
    }
    private void SetActiveAmmoShopItem(AmmoSlot ammoSlot) {
        _activeAmmoShopItem = ammoSlot;
        AnimateSelectionChanges();
        if (ammoSlot == null) return;
        AmmoData newAmmoData = ammoSlot.AmmoData;
        print("Active ammo shop item set to " + newAmmoData.AmmoName);
        
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
    
    
    /* Excludes ghost slot */
    private List<AmmoSlot> GetAllAmmoSlots() {
        List<AmmoSlot> allShopItems = new List<AmmoSlot>();
        foreach (Transform child in AmmoSlotContainer) {
            AmmoSlot ammoSlot = child.GetComponent<AmmoSlot>();
            if (ammoSlot == null) continue;
            allShopItems.Add(ammoSlot);
        }
        return allShopItems;
    }

    private Dictionary<AmmoData, int> GetUpgradeWithBagOfFreqs() {
        Dictionary<AmmoData,int> bagOfFreqs = new();
        foreach (AmmoSlot ammoSlot in GetAllAmmoSlots()) {
            AmmoData currAmmoData = ammoSlot.AmmoData;
            if (currAmmoData == null) {
                continue;
            }
            if (currAmmoData.UpgradeRecipe.CombineWith != null) {
                AmmoData entryInQuestion = currAmmoData.UpgradeRecipe.CombineWith;
                if (!bagOfFreqs.TryAdd(entryInQuestion, 1)) {
                    bagOfFreqs[entryInQuestion]++;
                }
            }
        }
        // Also count any upgrades slots that are currently in drag area
        RectTransform dragArea = PlayerBattleUIDelegates.GetDragLayerRectTransform?.Invoke();
        if (dragArea != null) {
            foreach (Transform child in dragArea.transform) {
                AmmoSlot ammoSlot = child.GetComponent<AmmoSlot>();
                if (ammoSlot == null) {
                    Debug.LogWarning("Current child in drag area has no ammo slot, skipping...");
                    continue;
                }
                AmmoData currAmmoData = ammoSlot.AmmoData;
                if (currAmmoData == null) {
                    Debug.LogError("Ammo slot has no ammo data!!");
                    continue;
                }
                if (currAmmoData.UpgradeRecipe.CombineWith != null) {
                    AmmoData entryInQuestion = currAmmoData.UpgradeRecipe.CombineWith;
                    if (!bagOfFreqs.TryAdd(entryInQuestion, 1)) {
                        print("incremented 1 in drag area");
                        bagOfFreqs[entryInQuestion]++;
                    }
                }
            }
        }
        return bagOfFreqs;
    }
    
    private void CheckForUpgrades() { // Do not use without delaying if you destroyed a gameobject!
        // Record all desired upgradeWith items
        Dictionary<AmmoData, int> bagOfFreqs = GetUpgradeWithBagOfFreqs();
        // Label all slots if they exist in the set
        foreach (AmmoSlot ammoSlot in GetAllAmmoSlots()) {
            ammoSlot.SetUpgradeIconVisibility(bagOfFreqs.ContainsKey(ammoSlot.AmmoData) && bagOfFreqs[ammoSlot.AmmoData] > 1);
        }
        
        // Also check drag area too
        RectTransform dragArea = PlayerBattleUIDelegates.GetDragLayerRectTransform?.Invoke();
        if (dragArea != null) {
            foreach (Transform child in dragArea.transform) {
                AmmoSlot ammoSlot = child.GetComponent<AmmoSlot>();
                if (ammoSlot != null) {
                    ammoSlot.SetUpgradeIconVisibility(bagOfFreqs.ContainsKey(ammoSlot.AmmoData) && bagOfFreqs[ammoSlot.AmmoData] > 1);
                }
            }
        }
    }

    private void CheckForUpgradesPatient() {
        StartCoroutine(DelayedCheckForUpgrades());
    }
    

    private void ResetAllAmmoSlotsCanvasGroupAlpha() {
        foreach (AmmoSlot ammoSlot in GetAllAmmoSlots()) {
            ammoSlot.GetComponent<CanvasGroup>().alpha = 1f;
        }
    }

    public void RemoveActiveAmmoShopItem() {
        Destroy(_activeAmmoShopItem.gameObject);
        _activeAmmoShopItem = null;
        StartCoroutine(DelayedCheckForUpgrades());
    }

    private void SpawnAmmoSlot(AmmoData ammoData) {
        GameObject newAmmoSlot = Instantiate(Resources.Load<GameObject>("Prefabs/UI/AmmoSlot"), AmmoSlotContainer);
        newAmmoSlot.transform.SetSiblingIndex(0);
        newAmmoSlot.GetComponent<AmmoSlot>().SetSlotData(ammoData);
        
        // Also account for if user is currently dragging something. If they are, change the alpha of the slot accordingly
        RectTransform dragArea = PlayerBattleUIDelegates.GetDragLayerRectTransform?.Invoke();
        if (dragArea != null) {
            if (dragArea.childCount > 0) {
                bool shouldBeInteractable = false;
                foreach (Transform child in dragArea) {
                    AmmoSlot ammoSlot = child.GetComponent<AmmoSlot>();
                    if (ammoSlot == null) continue;
                    if (ammoSlot.AmmoData == null) continue;
                    if (ammoSlot.AmmoData.UpgradeRecipe.CombineWith == null) continue;
                    if (ammoSlot.AmmoData.UpgradeRecipe.CombineWith == ammoData.UpgradeRecipe.CombineWith) {
                        shouldBeInteractable = true;
                    }
                }
                newAmmoSlot.GetComponent<CanvasGroup>().alpha = shouldBeInteractable ? 1 : 0.6f;
            }
        }
        
        
        CheckForUpgrades();
    }

    private void SpawnRandomAmmoSlot() {
        SpawnAmmoSlot(QuickUtils.Choice(GetAllAmmo()));
    }

    private AmmoData[] GetAllAmmo() {
        return Resources.LoadAll<AmmoData>("ScriptableObjects/Projectiles");
    }

    private void SetDropIndicatorSiblingIndex(int index) {
        DropIndicator.SetSiblingIndex(index);
    }

    private void SetDropIndicatorActive(bool active) {
        DropIndicator.gameObject.SetActive(active);
    }

    private void SetDropIndicatorParent(Transform parent, bool worldPositionStays) {
        DropIndicator.SetParent(parent, worldPositionStays);
    }
    
    private float GetPitchBasedOnResultRarity(Rarity rarity) {
        switch (rarity) {
            case Rarity.COMMON:
                return .8f;
            case Rarity.RARE:
                return .9f;
            case Rarity.EPIC:
                return 1f;
            case Rarity.LEGENDARY:
                return 1.1f;
            default:
                return 0f;
        }
    }

    private void DoAutoUpgrades() {
        if (_autoUpgradeCoroutine != null) {
            StopCoroutine(_autoUpgradeCoroutine);
            _autoUpgradeCoroutine = null;
        }
        _autoUpgradeCoroutine = StartCoroutine(AutoUpgradeCoroutine());
        
        
    }

    private bool CanAutoUpgrade() {
        List<AmmoSlot> allAmmoSlots = GetAllAmmoSlots();
        if (GetAllAmmoSlots().Count <= 1) return false;
        for (int i = 0; i < allAmmoSlots.Count; i++) {
            AmmoSlot currAmmoSlot = allAmmoSlots[i];
            AmmoSlot prevAmmoSlot = (i > 0) ? allAmmoSlots[i - 1] : null;
            AmmoSlot nextAmmoSlot = (i < allAmmoSlots.Count - 1) ? allAmmoSlots[i + 1] : null;
            AmmoData neededAmmoToUpgrade = currAmmoSlot.AmmoData.UpgradeRecipe.CombineWith;

            if (neededAmmoToUpgrade == null) continue;

            if (prevAmmoSlot != null && currAmmoSlot.AmmoData == neededAmmoToUpgrade) {
                if (prevAmmoSlot.AmmoData.UpgradeRecipe.CombineWith == null) continue;
                if (prevAmmoSlot.AmmoData.UpgradeRecipe.CombineWith != currAmmoSlot.AmmoData.UpgradeRecipe.CombineWith) continue;
                CombineAmmoSlots(prevAmmoSlot, currAmmoSlot);
                return true;
            }
            if (nextAmmoSlot != null && nextAmmoSlot.AmmoData == neededAmmoToUpgrade) {
                if (nextAmmoSlot.AmmoData.UpgradeRecipe.CombineWith == null) continue;
                if (nextAmmoSlot.AmmoData.UpgradeRecipe.CombineWith != currAmmoSlot.AmmoData.UpgradeRecipe.CombineWith) continue;
                CombineAmmoSlots(currAmmoSlot, nextAmmoSlot);
                return true;
            }

        }
        return false;
    }

    /* Merges left slot into right slot, effectively removing left slot and storing the result in right slot. */
    private void CombineAmmoSlots(AmmoSlot left, AmmoSlot right) {
        Debug.Log("Attempting to merge " + left.AmmoData.AmmoName + " into " + right.AmmoData.AmmoName);
        if (left == null || right == null) {
            Debug.LogError("Combine failed because left slot or right slot is null.");
            return;
        }
        if (left.AmmoData.UpgradeRecipe.CombineWith != right.AmmoData.UpgradeRecipe.CombineWith) {
            Debug.LogError("The two slots cannot be combined because the recipe does not match!");
            return;
        }
        
        // Attempt to combine
        AmmoData resultAmmo = right.AmmoData.UpgradeRecipe.UpgradesTo;
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/SFX/AmmoCombine"), GetPitchBasedOnResultRarity(resultAmmo.Rarity));
        
        // Remove the left ammo slot, and update the right ammo slot
        Destroy(left.gameObject);
        right.SetSlotData(resultAmmo);
        
        // Reassess upgrade available for all ammo
        PlayerBattleUIDelegates.InvokeOnCheckForUpgradesSetIcons();
        PlayerBattleUIDelegates.InvokeOnResetAllAmmoSlotsCanvasGroupAlpha();
    }

    private IEnumerator AutoUpgradeCoroutine() {
        while (CanAutoUpgrade()) {
            Debug.Log("Combining ammo");
            yield return null;
        }
        
    }
    
    private IEnumerator SupplyAmmoCoroutine() {
        while (true) {
            yield return new WaitForSeconds(Random.Range(2f, 4f));
            if (GetAllAmmoSlots().Count < 7) {
                SpawnRandomAmmoSlot();
            }
            
        }
    }
    
    private IEnumerator DelayedCheckForUpgrades() {
        yield return null;
        CheckForUpgrades();
    }

    private IEnumerator InitializeInput() {
        yield return null;
        // Create a certain amount of slots to start with
        for (int i = 0; i < 4; i++) {
            SpawnRandomAmmoSlot();
        }
        
        CheckForUpgrades();

        _supplyAmmoCoroutine = StartCoroutine(SupplyAmmoCoroutine());
    }
    
}
