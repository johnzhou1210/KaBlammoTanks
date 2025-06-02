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
   float _currentFireLoadingTime = 0.25f;

   AmmoData _loadedAmmoData;
   bool _firingInProgress = false;
   private float _loadingTimer = 0f;

   void Start() {
      originalSlotBackgroundColor = slotBackgroundImage.color;
   }

   void OnEnable() {
      _loadingTimer = -1;
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
      AmmoSlot selectedShopSlot = PlayerBattleInputDelegates.GetSelectedAmmoShopItem?.Invoke();
      if (selectedShopSlot != null) {
         // Check if player can afford before firing
         bool canAfford = true;
         if (canAfford) {
            AmmoData currAmmoData = selectedShopSlot.AmmoData;
            _currentFireLoadingTime = currAmmoData.LoadingTime;
            _loadingTimer = 0f;
            _firingInProgress = true;
            _loadedAmmoData = currAmmoData;
         }
      }
      slotBackgroundImage.color = originalSlotBackgroundColor;
   }

   public void OnPointerDown(PointerEventData eventData) {
      if (!GetIsReadyForInput()) return;
      slotBackgroundImage.color = Color.white;
   }

   void Update() {
      if (Mathf.Approximately(-1f, _loadingTimer)) return;
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
      loadingFill.fillAmount = _loadingTimer / _currentFireLoadingTime;
   }

   private bool GetIsReadyForInput() {
      return Mathf.Approximately(-1f, _loadingTimer) || _loadingTimer >= _currentFireLoadingTime;
   }

   private void InitiateFire(AmmoData ammoData) {
      TankDelegates.InvokeOnProjectileFire(ammoData, IsUpperCannon, 0);
   }

}
