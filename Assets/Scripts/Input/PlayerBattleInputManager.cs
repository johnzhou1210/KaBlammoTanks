using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerBattleInputManager : MonoBehaviour {
    private AmmoSlot _activeAmmoShopItem;
    [SerializeField] private Transform AmmoSlotContainer;

    private Coroutine _supplyAmmoCoroutine;
    
    void OnEnable() {
        PlayerBattleInputDelegates.OnShopAmmoTap += SetActiveAmmoShopItem;
        PlayerBattleInputDelegates.OnRemoveActiveAmmoShopItem += RemoveActiveAmmoShopItem;
        
        PlayerBattleUIDelegates.OnSetCombinerListenerEnabled += ToggleCombinerListeners;
        PlayerBattleUIDelegates.OnCheckForUpgradesSetIcons += CheckForUpgradesPatient;
        PlayerBattleUIDelegates.OnResetAllAmmoSlotsCanvasGroupAlpha += ResetAllAmmoSlotsCanvasGroupAlpha;
       

        PlayerBattleInputDelegates.GetSelectedAmmoShopItem = () => _activeAmmoShopItem;
    }
    void OnDisable() {
        PlayerBattleInputDelegates.OnShopAmmoTap -= SetActiveAmmoShopItem;
        PlayerBattleInputDelegates.OnRemoveActiveAmmoShopItem -= RemoveActiveAmmoShopItem;
        
        PlayerBattleUIDelegates.OnSetCombinerListenerEnabled -= ToggleCombinerListeners;
        PlayerBattleUIDelegates.OnCheckForUpgradesSetIcons -= CheckForUpgradesPatient;
        PlayerBattleUIDelegates.OnResetAllAmmoSlotsCanvasGroupAlpha -= ResetAllAmmoSlotsCanvasGroupAlpha;
        
        PlayerBattleInputDelegates.GetSelectedAmmoShopItem = null;
    }

    void Start() {
    
        // Create a certain amount of slots to start with
        for (int i = 0; i < 4; i++) {
            SpawnRandomAmmoSlot();
        }
        
        ToggleCombinerListeners(false);
        CheckForUpgrades();

        _supplyAmmoCoroutine = StartCoroutine(SupplyAmmoCoroutine());
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

    private void ToggleCombinerListeners(bool val, AmmoData combineWith = null) {
        if (combineWith == null) {
            // Set all combiner listeners to false, ignoring val
            foreach (AmmoSlot ammoSlot in GetAllAmmoSlots()) {
                AmmoUpgradeCombinerListener combinerListener = ammoSlot.GetComponent<AmmoUpgradeCombinerListener>();
                if (combinerListener == null) {
                    Debug.LogError("Ammo slot has no ammo upgrade combiner listener!");
                    continue;
                }
                combinerListener.enabled = false;
            }
        }
        
        // Only enable the combiner listeners that match upgradeWith
        foreach (AmmoSlot ammoSlot in GetAllAmmoSlots()) {
            if (ammoSlot.AmmoData == combineWith) {
                AmmoUpgradeCombinerListener combinerListener = ammoSlot.GetComponent<AmmoUpgradeCombinerListener>();
                if (combinerListener == null) {
                    Debug.LogError("Ammo slot has no ammo upgrade combiner listener!");
                    continue;
                }
                combinerListener.enabled = val;
            }
        }
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
    
    private IEnumerator SupplyAmmoCoroutine() {
        while (true) {
            yield return new WaitForSeconds(Random.Range(2f, 4f));
            if (GetAllAmmoSlots().Count < 10) {
                SpawnRandomAmmoSlot();
            }
            
        }
    }
    
    private IEnumerator DelayedCheckForUpgrades() {
        yield return null;
        CheckForUpgrades();
    }
    
}
