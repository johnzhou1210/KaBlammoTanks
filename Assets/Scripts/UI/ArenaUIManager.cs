using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ArenaUIManager : MonoBehaviour
{
    public static ArenaUIManager Instance;
    [SerializeField] private GameObject ammoSlotContainer, dragLayer, ammoShop, tankBillboards;
    [SerializeField] private Camera mainCamera, hostFocusedCamera, hosteeFocusedCamera;
    [SerializeField] private TextMeshProUGUI battleStatusPopupText;
    
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void HideBattleUI() {
        CanvasGroup ammoSlotContainerCanvasGroup = ammoSlotContainer.GetComponent<CanvasGroup>();
        CanvasGroup ammoShopCanvasGroup = ammoShop.GetComponent<CanvasGroup>();

        ammoSlotContainerCanvasGroup.interactable = false;
        ammoSlotContainerCanvasGroup.blocksRaycasts = false;
        ammoShopCanvasGroup.interactable = false;
        ammoShopCanvasGroup.blocksRaycasts = false;
        
        // Hide drag layer, ammo slot container, and ammo shop
        ammoSlotContainerCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InCubic).SetUpdate(true);
        ammoShopCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InCubic).SetUpdate(true);
       dragLayer.SetActive(false);
       
    }

    public void ShowBattleUI() {
        CanvasGroup ammoSlotContainerCanvasGroup = ammoSlotContainer.GetComponent<CanvasGroup>();
        CanvasGroup ammoShopCanvasGroup = ammoShop.GetComponent<CanvasGroup>();

        ammoSlotContainerCanvasGroup.interactable = true;
        ammoSlotContainerCanvasGroup.blocksRaycasts = true;
        ammoShopCanvasGroup.interactable = true;
        ammoShopCanvasGroup.blocksRaycasts = true;
        
        // Hide drag layer, ammo slot container, and ammo shop
        ammoSlotContainerCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.InCubic);
        ammoShopCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.InCubic);
        dragLayer.SetActive(true);
    }
    
    public void ShowWinScreen() {
        Debug.Log("Showing win screen");
        battleStatusPopupText.text = "VICTORY";
    }

    public void ShowLoseScreen() {
        Debug.Log("Showing lose screen");
        battleStatusPopupText.text = "DEFEAT";
    }

    public void FocusMainCamera() {
        mainCamera.enabled = true;
        hostFocusedCamera.enabled = false;
        hosteeFocusedCamera.enabled = false;
    }

    public void FocusHostCamera() {
        mainCamera.enabled = false;
        hostFocusedCamera.enabled = true;
        hosteeFocusedCamera.enabled = false;
    }

    public void FocusHosteeCamera() {
        mainCamera.enabled = false;
        hostFocusedCamera.enabled = false;
        hosteeFocusedCamera.enabled = true;
    }

    public Camera GetMainCamera() {
        return mainCamera;
    }
}
