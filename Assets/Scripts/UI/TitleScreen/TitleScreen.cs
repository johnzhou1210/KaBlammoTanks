using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour, IPointerDownHandler {
    [SerializeField] private GameObject startPrompt;

    public void OnPointerDown(PointerEventData eventData) {
        if (startPrompt.activeInHierarchy) {
            SceneManager.LoadScene(1);
        }
    }

    private void Start() {
        StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine() {
        yield return new WaitForSeconds(6f);
        startPrompt.SetActive(true);
        float startTime = 0;
        float currTime = startTime;
        while (startPrompt.activeInHierarchy) {
            currTime += Time.deltaTime;
            startPrompt.GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 1f, Mathf.Sin(currTime * 6f) + 1f);
            yield return new WaitForEndOfFrame();
        }
    }

}
