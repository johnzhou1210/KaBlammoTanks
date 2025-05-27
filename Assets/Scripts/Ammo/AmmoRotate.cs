using System;
using UnityEngine;

public class AmmoRotate : MonoBehaviour {
    public float RotationSpeed = 90f;
    void Update() {
        transform.Rotate(Vector3.forward, RotationSpeed * Time.deltaTime);
    }

    public void ReverseRotation() {
        RotationSpeed = -RotationSpeed;
    }
    
}
