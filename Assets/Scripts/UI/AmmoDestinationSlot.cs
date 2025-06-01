using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AmmoDestinationSlot : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
   [field: SerializeField] public bool IsUpperCannon { get; private set; }
   [SerializeField] Image slotImage, slotBackgroundImage, loadingFill;
   AmmoData _ammoData;
   Color originalSlotBackgroundColor;
   [SerializeField] float fireLoadingTime = 0.25f;

   AmmoData _loadedAmmoData;
   bool _firingInProgress = false;
   private float _loadingTimer = 0f;

   void Start() {
      originalSlotBackgroundColor = slotBackgroundImage.color;
   }

   void OnEnable() {
      _loadingTimer = fireLoadingTime;
      PlayerBattleUIDelegates.OnSetAmmoDestinationSlot += SetAmmoData;
   }

   void OnDisable() {
      PlayerBattleUIDelegates.OnSetAmmoDestinationSlot -= SetAmmoData;
   }

   private void SetAmmoData(bool isUpperCannon, AmmoData ammoData) {
      if (IsUpperCannon != isUpperCannon) return;
      _ammoData = ammoData;
      slotImage.sprite = _ammoData.Icon;
   }

   public void OnPointerUp(PointerEventData eventData) {
      if (!GetIsReadyForInput()) return;
      // Set ammo only if Player Battle Input Manager has ammo data
      AmmoTapHandler selectedTapHandler = PlayerBattleInputDelegates.GetSelectedAmmoShopItem?.Invoke();
      if (selectedTapHandler != null) {
         // Check if player can afford before firing
         bool canAfford = true;
         if (canAfford) {
            _loadingTimer = 0f;
            _firingInProgress = true;
            _loadedAmmoData = selectedTapHandler.AmmoData;
         }
      }
      slotBackgroundImage.color = originalSlotBackgroundColor;
   }

   public void OnPointerDown(PointerEventData eventData) {
      if (!GetIsReadyForInput()) return;
      slotBackgroundImage.color = Color.white;
   }

   void Update() {
      if (GetIsReadyForInput()) {
         loadingFill.fillAmount = 0f;
         if (_firingInProgress) {
            _firingInProgress = false;
            InitiateFire(_loadedAmmoData);
            _loadedAmmoData = null;
         }
         return;  
      }
      _loadingTimer += Time.deltaTime;
      loadingFill.fillAmount = _loadingTimer / fireLoadingTime;
   }

   private bool GetIsReadyForInput() {
      return _loadingTimer >= fireLoadingTime;
   }

   private void InitiateFire(AmmoData ammoData) {
      TankDelegates.InvokeOnProjectileFire(ammoData, IsUpperCannon, 0);
   }

}
