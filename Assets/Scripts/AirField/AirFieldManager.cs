using KBCore.Refs;
using UnityEngine;

public class AirFieldManager : MonoBehaviour {
    [SerializeField] [Self] private Transform _airfieldTransform;
    [SerializeField] private Transform _debrisTransform;

    private void OnEnable() {
        AirFieldDelegates.GetAirFieldTransform = () => _airfieldTransform;
        AirFieldDelegates.GetDebrisTransform = () => _debrisTransform;
    }

    private void OnDisable() {
        AirFieldDelegates.GetAirFieldTransform = null;
        AirFieldDelegates.GetDebrisTransform = null;
    }

    private void OnValidate() {
        this.ValidateRefs();
    }
}