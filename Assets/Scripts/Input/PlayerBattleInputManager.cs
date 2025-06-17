using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class PlayerBattleInputManager : MonoBehaviour {
    [SerializeField] private Transform AmmoSlotContainer;
    [SerializeField] private RectTransform DropIndicator;
    [SerializeField] private float AmmoUpgradeCooldown = 1f;
    private AmmoSlot _activeAmmoShopItem;
    private float _ammoUpgradeCooldownTimer;
    private int _autoUpgradeChecksToDo;

    private Coroutine _supplyAmmoCoroutine, _ammoUpgradeCoroutine;


    private void Start() {
        EventSystem.current.pixelDragThreshold = 25;
        StartCoroutine(InitializeInput());
    }


    private void Update() {
        _ammoUpgradeCooldownTimer = Mathf.Clamp(_ammoUpgradeCooldownTimer + Time.deltaTime, -1f, AmmoUpgradeCooldown);
        if (_autoUpgradeChecksToDo > 0) {
            if (_ammoUpgradeCooldownTimer < AmmoUpgradeCooldown) return;
            _ammoUpgradeCooldownTimer = 0 - AmmoUpgradeCooldown;

            // Do single upgrade
            bool upgradeSuccess = CanAutoUpgrade();
            _autoUpgradeChecksToDo += upgradeSuccess ? 1 : 0;
        }
    }

    private void OnEnable() {
        PlayerBattleInputDelegates.OnShopAmmoTap += SetActiveAmmoShopItem;
        PlayerBattleInputDelegates.OnRemoveActiveAmmoShopItem += RemoveActiveAmmoShopItem;
        PlayerBattleInputDelegates.OnDelayedCheckForUpgrades += DelayedCheckForUpgrades;
        PlayerBattleInputDelegates.OnRequestUpgradeCheck += RequestUpgradeCheck;

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
        PlayerBattleInputDelegates.OnRequestUpgradeCheck -= RequestUpgradeCheck;

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
        AmmoData newAmmoData = ammoSlot.AmmoData;
        print("Active ammo shop item set to " + newAmmoData.AmmoName);
    }

    private void AnimateSelectionChanges() {
        var allShopItems = GetAllAmmoSlots();
        foreach (AmmoSlot shopItem in allShopItems) {
            Animator currAnimator = shopItem.GetComponent<Animator>();
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
            AmmoSlot ammoSlot = child.GetComponentInChildren<AmmoSlot>();
            if (ammoSlot == null) continue;
            allShopItems.Add(ammoSlot);
        }

        return allShopItems;
    }

    private Dictionary<AmmoData, int> GetUpgradeWithBagOfFreqs() {
        Dictionary<AmmoData, int> bagOfFreqs = new();
        foreach (AmmoSlot ammoSlot in GetAllAmmoSlots()) {
            AmmoData currAmmoData = ammoSlot.AmmoData;
            if (currAmmoData == null) continue;
            if (currAmmoData.UpgradeRecipe.CombineWith != null) {
                AmmoData entryInQuestion = currAmmoData.UpgradeRecipe.CombineWith;
                if (!bagOfFreqs.TryAdd(entryInQuestion, 1)) bagOfFreqs[entryInQuestion]++;
            }
        }

        // Also count any upgrades slots that are currently in drag area
        RectTransform dragArea = PlayerBattleUIDelegates.GetDragLayerRectTransform?.Invoke();
        if (dragArea != null)
            foreach (Transform child in dragArea.transform) {
                AmmoSlot ammoSlot = child.GetComponentInChildren<AmmoSlot>();
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

        return bagOfFreqs;
    }

    private void CheckForUpgrades() {
        // Do not use without delaying if you destroyed a gameobject!
        // Record all desired upgradeWith items
        var bagOfFreqs = GetUpgradeWithBagOfFreqs();
        // Label all slots if they exist in the set
        foreach (AmmoSlot ammoSlot in GetAllAmmoSlots())
            ammoSlot.SetUpgradeIconVisibility(bagOfFreqs.ContainsKey(ammoSlot.AmmoData) &&
                                              bagOfFreqs[ammoSlot.AmmoData] > 1);

        // Also check drag area too
        RectTransform dragArea = PlayerBattleUIDelegates.GetDragLayerRectTransform?.Invoke();
        if (dragArea != null)
            foreach (Transform child in dragArea.transform) {
                AmmoSlot ammoSlot = child.GetComponentInChildren<AmmoSlot>();
                if (ammoSlot != null)
                    ammoSlot.SetUpgradeIconVisibility(bagOfFreqs.ContainsKey(ammoSlot.AmmoData) &&
                                                      bagOfFreqs[ammoSlot.AmmoData] > 1);
            }
    }

    private void CheckForUpgradesPatient() {
        StartCoroutine(DelayedCheckForUpgradesCoroutine());
    }


    private void ResetAllAmmoSlotsCanvasGroupAlpha() {
        foreach (AmmoSlot ammoSlot in GetAllAmmoSlots()) ammoSlot.GetComponent<CanvasGroup>().alpha = 1f;
    }

    private void RemoveActiveAmmoShopItem() {
        Debug.Log("IN HERE");
        _activeAmmoShopItem.GetComponent<Animator>().Play("AmmoCardConsume");
        Debug.Log("Removed ammo shop item");
        _activeAmmoShopItem = null;
    }

    private void SpawnAmmoSlot(AmmoData ammoData) {
        GameObject newAmmoSlot = Instantiate(Resources.Load<GameObject>("Prefabs/UI/CardItem"), AmmoSlotContainer);
        newAmmoSlot.transform.SetSiblingIndex(0);
        newAmmoSlot.GetComponentInChildren<AmmoSlot>().SetSlotData(ammoData);

        // Also account for if user is currently dragging something. If they are, change the alpha of the slot accordingly
        RectTransform dragArea = PlayerBattleUIDelegates.GetDragLayerRectTransform?.Invoke();
        if (dragArea != null)
            if (dragArea.childCount > 0) {
                bool shouldBeInteractable = false;
                foreach (Transform child in dragArea) {
                    AmmoSlot ammoSlot = child.GetComponentInChildren<AmmoSlot>();
                    if (ammoSlot == null) continue;
                    if (ammoSlot.AmmoData == null) continue;
                    if (ammoSlot.AmmoData.UpgradeRecipe.CombineWith == null) continue;
                    if (ammoSlot.AmmoData.UpgradeRecipe.CombineWith == ammoData.UpgradeRecipe.CombineWith)
                        shouldBeInteractable = true;
                }

                newAmmoSlot.GetComponentInChildren<CanvasGroup>().alpha = shouldBeInteractable ? 1 : 0.6f;
            }


        CheckForUpgrades();
        _autoUpgradeChecksToDo += !ammoData.UpgradeRecipe.CombineWith ? 0 : 1;

        // Reset cooldown to let player react to the spawn
    }

    private void SpawnRandomAmmoSlot(AmmoData[] ammoPool) {
        SpawnAmmoSlot(QuickUtils.Choice(ammoPool));
    }

    private AmmoData[] GetAllAmmo() {
        return Resources.LoadAll<AmmoData>("ScriptableObjects/Projectiles");
    }

    private AmmoData[] GetAmmoPool(string scriptableObjectPath) {
        AmmoPool ammoPool = Resources.Load<AmmoPool>(scriptableObjectPath);
        return ammoPool.Contents;
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

    private void RequestUpgradeCheck() {
        // Reset upgrade timer to avoid inconsistent cooldowns, but is a shorter cooldown
        _ammoUpgradeCooldownTimer = 0f;
        _autoUpgradeChecksToDo += 1;
    }

    private bool CanAutoUpgrade() {
        // Skip check if player is dragging
        if (!Mathf.Approximately(AmmoSlotContainer.GetComponent<CanvasGroup>().alpha, 1f)) return false;

        var allAmmoSlots = GetAllAmmoSlots();
        if (allAmmoSlots == null) {
            Debug.LogWarning("AllAmmoSlots is null!");
            return false;
        }

        if (GetAllAmmoSlots().Count <= 1)
            return false;
        for (int i = 0; i < allAmmoSlots.Count; i++) {
            AmmoSlot currAmmoSlot = allAmmoSlots[i];
            AmmoSlot prevAmmoSlot = i > 0 ? allAmmoSlots[i - 1] : null;
            AmmoSlot nextAmmoSlot = i < allAmmoSlots.Count - 1 ? allAmmoSlots[i + 1] : null;
            AmmoData neededAmmoToUpgrade = currAmmoSlot.AmmoData.UpgradeRecipe.CombineWith;
            if (neededAmmoToUpgrade == null)
                continue;

            // Prioritize combining with previous ammo slot first
            if (prevAmmoSlot != null && currAmmoSlot.AmmoData == neededAmmoToUpgrade) {
                if (prevAmmoSlot.AmmoData.UpgradeRecipe.CombineWith == null)
                    continue;
                if (prevAmmoSlot.AmmoData.UpgradeRecipe.CombineWith != currAmmoSlot.AmmoData.UpgradeRecipe.CombineWith)
                    continue;
                MergeSlotsWithAnimation(prevAmmoSlot, currAmmoSlot);
                return true;
            }

            // Check with combining with next ammo slot if previous ammo slot cannot combine
            if (nextAmmoSlot != null && nextAmmoSlot.AmmoData == neededAmmoToUpgrade) {
                if (nextAmmoSlot.AmmoData.UpgradeRecipe.CombineWith == null)
                    continue;
                if (nextAmmoSlot.AmmoData.UpgradeRecipe.CombineWith != currAmmoSlot.AmmoData.UpgradeRecipe.CombineWith)
                    continue;
                MergeSlotsWithAnimation(currAmmoSlot, nextAmmoSlot);
                return true;
            }
        }

        return false;
    }


    private void MergeSlotsWithAnimation(AmmoSlot left, AmmoSlot right) {
        left.SetIsInteractable(false);
        right.SetIsInteractable(false);
        StartCoroutine(MergeSlotCoroutine(left, right));
    }


    private IEnumerator SupplyAmmoCoroutine() {
        while (true) {
            yield return new WaitForSeconds(Random.Range(1f, 3f));
            if (GetAllAmmoSlots().Count < 7)
                SpawnRandomAmmoSlot(GetAmmoPool("ScriptableObjects/AmmoPools/DefaultPool"));
        }
    }

    private IEnumerator DelayedCheckForUpgradesCoroutine() {
        yield return null;
        CheckForUpgrades();
    }

    private IEnumerator InitializeInput() {
        yield return null;
        // Create a certain amount of slots to start with
        for (int i = 0; i < 4; i++) SpawnRandomAmmoSlot(GetAmmoPool("ScriptableObjects/AmmoPools/DefaultPool"));

        CheckForUpgrades();

        _supplyAmmoCoroutine = StartCoroutine(SupplyAmmoCoroutine());
    }

    private IEnumerator MergeSlotCoroutine(AmmoSlot left, AmmoSlot right) {
        RectTransform dragLayerTransform = PlayerBattleUIDelegates.GetDragLayerRectTransform?.Invoke();
        if (dragLayerTransform == null) {
            Debug.LogError("Drag Layer transform is null!");
            yield break;
        }

        Animator leftAnimator = left.GetComponent<Animator>();
        Animator rightAnimator = right.GetComponent<Animator>();

        left.transform.parent.SetParent(dragLayerTransform, true);
        leftAnimator.Play("AmmoCardMerger");
        rightAnimator.StopPlayback();
        rightAnimator.Play("AmmoCardMergee");

        StartCoroutine(WaitAndCompleteMergeCoroutine(left, right));

        yield return null;
    }

    private IEnumerator WaitAndCompleteMergeCoroutine(AmmoSlot left, AmmoSlot right) {
        yield return new WaitForSeconds(0.25f / .6f);
        AmmoData resultAmmo = right.AmmoData.UpgradeRecipe.UpgradesTo;
        // Remove the left ammo slot, and update the right ammo slot
        Destroy(left.transform.parent.gameObject);
        right.SetSlotData(resultAmmo);
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/SFX/AmmoCombine"),
            right.GetComponent<AmmoUpgradeCombinerHandler>().GetPitchBasedOnResultRarity(resultAmmo.Rarity));
        // Reassess upgrade available for all ammo
        PlayerBattleUIDelegates.InvokeOnCheckForUpgradesSetIcons();
        PlayerBattleUIDelegates.InvokeOnResetAllAmmoSlotsCanvasGroupAlpha();

        right.SetIsInteractable(true);

        yield return new WaitForSeconds(1f);
        _autoUpgradeChecksToDo += 1;
    }
}