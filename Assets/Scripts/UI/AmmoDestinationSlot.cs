using System;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
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

   AmmoRequest _loadedAmmoRequest;
   bool _firingInProgress = false;
   private float _loadingTimer = 0f;
   private TankController _localTank;

   void Start() {
      originalSlotBackgroundColor = slotBackgroundImage.color;
      _localTank = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<TankController>();
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
            AmmoRequest currAmmoRequest = new AmmoRequest {
               AmmoName = currAmmoData.AmmoName,
               IsValid = true
            };
            _loadedAmmoRequest = currAmmoRequest;
            selectedShopSlot.SetIsInteractable(false);
            PlayerBattleInputDelegates.InvokeOnRemoveActiveAmmoShopItem();
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
            InitiateFire(_loadedAmmoRequest);
            _loadedAmmoRequest.IsValid = false;
         }
         return;  
      }
      _loadingTimer += Time.deltaTime;
      loadingFill.fillAmount = _loadingTimer / _currentFireLoadingTime;
   }

   private bool GetIsReadyForInput() {
      return Mathf.Approximately(-1f, _loadingTimer) || _loadingTimer >= _currentFireLoadingTime;
   }

   private void InitiateFire(AmmoRequest ammoRequest) {
      // TankDelegates.InvokeOnProjectileFire(ammoRequest, IsUpperCannon);
      if (_localTank != null && _localTank.IsOwner) {
         _localTank.FireProjectileServerRpc(ammoRequest, IsUpperCannon);
      }
      
   }

}
