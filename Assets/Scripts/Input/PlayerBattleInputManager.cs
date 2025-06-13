using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class PlayerBattleInputManager : MonoBehaviour {
    [SerializeField] private Transform AmmoSlotContainer;
    [SerializeField] private RectTransform DropIndicator;
    private AmmoSlot _activeAmmoShopItem;
    private bool _isDragging = false;

    private Coroutine _supplyAmmoCoroutine;

    private void Start() {
        EventSystem.current.pixelDragThreshold = 25;
        StartCoroutine(InitializeInput());
        Debug.Log("IN HERE333");
    }

    private void OnEnable() {
        PlayerBattleInputDelegates.OnShopAmmoTap += SetActiveAmmoShopItem;
        PlayerBattleInputDelegates.OnRemoveActiveAmmoShopItem += RemoveActiveAmmoShopItem;
        PlayerBattleInputDelegates.OnDelayedCheckForUpgrades += DelayedCheckForUpgrades;

        PlayerBattleUIDelegates.OnCheckForUpgradesSetIcons += CheckForUpgradesPatient;
        PlayerBattleUIDelegates.OnResetAllAmmoSlotsCanvasGroupAlpha += ResetAllAmmoSlotsCanvasGroupAlpha;
        PlayerBattleUIDelegates.OnSetDropIndicatorSiblingIndex += SetDropIndicatorSiblingIndex;
        PlayerBattleUIDelegates.OnSetDropIndicatorActive += SetDropIndicatorActive;
        PlayerBattleUIDelegates.OnDropIndicatorSetParent += SetDropIndicatorParent;


        PlayerBattleInputDelegates.GetSelectedAmmoShopItem = () => _activeAmmoShopItem;
        PlayerBattleInputDelegates.GetAllAmmoSlots = GetAllAmmoSlots;
    }

    private void OnDisable() {
        PlayerBattleInputDelegates.OnShopAmmoTap -= SetActiveAmmoShopItem;
        PlayerBattleInputDelegates.OnRemoveActiveAmmoShopItem -= RemoveActiveAmmoShopItem;
        PlayerBattleInputDelegates.OnDelayedCheckForUpgrades -= DelayedCheckForUpgrades;

        PlayerBattleUIDelegates.OnCheckForUpgradesSetIcons -= CheckForUpgradesPatient;
        PlayerBattleUIDelegates.OnResetAllAmmoSlotsCanvasGroupAlpha -= ResetAllAmmoSlotsCanvasGroupAlpha;
        PlayerBattleUIDelegates.OnSetDropIndicatorSiblingIndex -= SetDropIndicatorSiblingIndex;
        PlayerBattleUIDelegates.OnSetDropIndicatorActive -= SetDropIndicatorActive;
        PlayerBattleUIDelegates.OnDropIndicatorSetParent -= SetDropIndicatorParent;

        PlayerBattleInputDelegates.GetSelectedAmmoShopItem = null;
        PlayerBattleInputDelegates.GetAllAmmoSlots = null;
    }

    private void SetActiveAmmoShopItem(AmmoSlot ammoSlot) {
        _activeAmmoShopItem = ammoSlot;
        AnimateSelectionChanges();
        if (ammoSlot == null) return;
        var newAmmoData = ammoSlot.AmmoData;
        print("Active ammo shop item set to " + newAmmoData.AmmoName);
    }

    private void AnimateSelectionChanges() {
        var allShopItems = GetAllAmmoSlots();
        foreach (var shopItem in allShopItems) {
            var currAnimator = shopItem.GetComponent<Animator>();
            if (shopItem != _activeAmmoShopItem) {
                // Do nothing, except if the item's animator is still playing selected animation
                if (GetIsSelectionAnimationPlaying(currAnimator)) currAnimator.Play("AmmoShopUIDeselect");
            } else {
                // Play select animation if ui is not already playing select animation
                if (!GetIsSelectionAnimationPlaying(currAnimator)) currAnimator.Play("AmmoShopUISelect");
            }
        }
    }

    private bool GetIsSelectionAnimationPlaying(Animator animator) {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("AmmoShopUISelected") ||
               animator.GetCurrentAnimatorStateInfo(0).IsName("AmmoShopUISelect");
    }


    /* Excludes ghost slot */
    private List<AmmoSlot> GetAllAmmoSlots() {
        var allShopItems = new List<AmmoSlot>();
        foreach (Transform child in AmmoSlotContainer) {
            var ammoSlot = child.GetComponentInChildren<AmmoSlot>();
            if (ammoSlot == null) continue;
            allShopItems.Add(ammoSlot);
        }

        return allShopItems;
    }

    private Dictionary<AmmoData, int> GetUpgradeWithBagOfFreqs() {
        Dictionary<AmmoData, int> bagOfFreqs = new();
        foreach (var ammoSlot in GetAllAmmoSlots()) {
            var currAmmoData = ammoSlot.AmmoData;
            if (currAmmoData == null) continue;
            if (currAmmoData.UpgradeRecipe.CombineWith != null) {
                var entryInQuestion = currAmmoData.UpgradeRecipe.CombineWith;
                if (!bagOfFreqs.TryAdd(entryInQuestion, 1)) bagOfFreqs[entryInQuestion]++;
            }
        }

        // Also count any upgrades slots that are currently in drag area
        var dragArea = PlayerBattleUIDelegates.GetDragLayerRectTransform?.Invoke();
        if (dragArea != null)
            foreach (Transform child in dragArea.transform) {
                var ammoSlot = child.GetComponentInChildren<AmmoSlot>();
                if (ammoSlot == null) {
                    Debug.LogWarning("Current child in drag area has no ammo slot, skipping...");
                    continue;
                }

                var currAmmoData = ammoSlot.AmmoData;
                if (currAmmoData == null) {
                    Debug.LogError("Ammo slot has no ammo data!!");
                    continue;
                }

                if (currAmmoData.UpgradeRecipe.CombineWith != null) {
                    var entryInQuestion = currAmmoData.UpgradeRecipe.CombineWith;
                    if (!bagOfFreqs.TryAdd(entryInQuestion, 1)) {
                        print("incremented 1 in drag area");
                        bagOfFreqs[entryInQuestion]++;
                    }
                }
            }

        return bagOfFreqs;
    }

    private void CheckForUpgrades() {
        // Do not use without delaying if you destroyed a gameobject!
        // Record all desired upgradeWith items
        var bagOfFreqs = GetUpgradeWithBagOfFreqs();
        // Label all slots if they exist in the set
        foreach (var ammoSlot in GetAllAmmoSlots())
            ammoSlot.SetUpgradeIconVisibility(bagOfFreqs.ContainsKey(ammoSlot.AmmoData) &&
                                              bagOfFreqs[ammoSlot.AmmoData] > 1);

        // Also check drag area too
        var dragArea = PlayerBattleUIDelegates.GetDragLayerRectTransform?.Invoke();
        if (dragArea != null)
            foreach (Transform child in dragArea.transform) {
                var ammoSlot = child.GetComponentInChildren<AmmoSlot>();
                if (ammoSlot != null)
                    ammoSlot.SetUpgradeIconVisibility(bagOfFreqs.ContainsKey(ammoSlot.AmmoData) &&
                                                      bagOfFreqs[ammoSlot.AmmoData] > 1);
            }
    }

    private void CheckForUpgradesPatient() {
        StartCoroutine(DelayedCheckForUpgradesCoroutine());
    }


    private void ResetAllAmmoSlotsCanvasGroupAlpha() {
        foreach (var ammoSlot in GetAllAmmoSlots()) ammoSlot.GetComponent<CanvasGroup>().alpha = 1f;
    }

    private void RemoveActiveAmmoShopItem() {
        Debug.Log("IN HERE");
        _activeAmmoShopItem.GetComponent<Animator>().Play("AmmoCardConsume");
        Debug.Log("Removed ammo shop item");
        // _activeAmmoShopItem = null;
    }

    private void SpawnAmmoSlot(AmmoData ammoData) {
        var newAmmoSlot = Instantiate(Resources.Load<GameObject>("Prefabs/UI/CardItem"), AmmoSlotContainer);
        newAmmoSlot.transform.SetSiblingIndex(0);
        newAmmoSlot.GetComponentInChildren<AmmoSlot>().SetSlotData(ammoData);

        // Also account for if user is currently dragging something. If they are, change the alpha of the slot accordingly
        var dragArea = PlayerBattleUIDelegates.GetDragLayerRectTransform?.Invoke();
        if (dragArea != null)
            if (dragArea.childCount > 0) {
                var shouldBeInteractable = false;
                foreach (Transform child in dragArea) {
                    var ammoSlot = child.GetComponentInChildren<AmmoSlot>();
                    if (ammoSlot == null) continue;
                    if (ammoSlot.AmmoData == null) continue;
                    if (ammoSlot.AmmoData.UpgradeRecipe.CombineWith == null) continue;
                    if (ammoSlot.AmmoData.UpgradeRecipe.CombineWith == ammoData.UpgradeRecipe.CombineWith)
                        shouldBeInteractable = true;
                }

                newAmmoSlot.GetComponentInChildren<CanvasGroup>().alpha = shouldBeInteractable ? 1 : 0.6f;
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

    private void DelayedCheckForUpgrades() {
        StartCoroutine(DelayedCheckForUpgradesCoroutine());
    }


    private IEnumerator SupplyAmmoCoroutine() {
        while (true) {
            yield return new WaitForSeconds(Random.Range(2f, 4f));
            if (GetAllAmmoSlots().Count < 7) SpawnRandomAmmoSlot();
        }
    }

    private IEnumerator DelayedCheckForUpgradesCoroutine() {
        yield return null;
        CheckForUpgrades();
    }

    private IEnumerator InitializeInput() {
        yield return null;
        // Create a certain amount of slots to start with
        for (var i = 0; i < 4; i++) SpawnRandomAmmoSlot();

        CheckForUpgrades();

        _supplyAmmoCoroutine = StartCoroutine(SupplyAmmoCoroutine());
    }
}