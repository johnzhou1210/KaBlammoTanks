using System;
using QFSW.QC;
using UnityEngine;

public class QuantumConsoleConditionalRender : MonoBehaviour {
   [SerializeField] private GameObject consoleGO;
   private void Start() {
      #if !UNITY_EDITOR
      consoleGO.SetActive(true);
      #endif
   }


}
