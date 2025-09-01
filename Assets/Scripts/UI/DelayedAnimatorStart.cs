using System;
using System.Collections;
using KBCore.Refs;
using UnityEngine;

public class DelayedAnimatorStart : MonoBehaviour
{
    [SerializeField, Self] private Animator animator;
    [SerializeField] private float startDelay = 4f;
    private void OnValidate() {
        this.ValidateRefs();
    }

    private void Start() {
        Invoke(nameof(StartAnimator), startDelay);
    }

    private void StartAnimator() {
        animator.enabled = true;
    }
}
