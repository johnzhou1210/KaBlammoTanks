using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAspectAdjuster : MonoBehaviour
{
    [SerializeField] private float baseOrthographicSize = 5.2f; // designed for target aspect
    private Camera cam;
    private const float targetAspect = 16f / 9f;


    private void Awake()
    {
        cam = GetComponent<Camera>();
        
        
    }

    private void Start() {
        AdjustCamera();
    }

    private void AdjustCamera()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        cam.orthographicSize = baseOrthographicSize;

        if (currentAspect > targetAspect)
        {
            // Device is wider: increase vertical size to show all horizontal content
            cam.orthographicSize = baseOrthographicSize * (currentAspect / targetAspect);
        }

        // If device is narrower than target, you can keep the base size or tweak as needed
    }
}
