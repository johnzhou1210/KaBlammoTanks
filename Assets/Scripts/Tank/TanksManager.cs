using System;
using System.Linq;
using UnityEngine;

public class TanksManager : MonoBehaviour
{
    [SerializeField] private TankController[] tankControllers;
    void OnEnable() {
        TankDelegates.GetTankControllerById = (id) => {
            return tankControllers.FirstOrDefault(e => e.TankId == id);
        };

        TankDelegates.GetTankHealthById = (id) => {
            TankController controller = TankDelegates.GetTankControllerById?.Invoke(id) ?? null;
            if (controller == null) {
                Debug.LogError("Tank controller not found");
                return -1;
            }
            return controller.TankHealth;
        };
        
        TankDelegates.GetTankMaxHealthById = (id) => {
            TankController controller = TankDelegates.GetTankControllerById?.Invoke(id) ?? null;
            if (controller == null) {
                Debug.LogError("Tank controller not found");
                return -1;
            }
            return controller.TankMaxHealth;
        };


    }

    void OnDisable() {
        TankDelegates.GetTankControllerById = null;
        TankDelegates.GetTankHealthById = null;
        TankDelegates.GetTankMaxHealthById = null;
    }

}
