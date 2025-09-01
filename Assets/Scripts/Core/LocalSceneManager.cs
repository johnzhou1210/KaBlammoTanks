using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LocalSceneManager : MonoBehaviour
{
    public static LocalSceneManager Instance;
    public bool IsSoloPlay { get; private set; }

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        LoadTitleScene();
    }

    public void ResubscribeToSceneVerificationEvents() {
        StartCoroutine(ConnectSceneVerificationEvents());
    }

    public void SetSoloPlay(bool val) {
        IsSoloPlay = val;
    }

    private bool VerifyScene(int sceneIndex, string sceneName, LoadSceneMode loadSceneMode) {
        if (sceneName == "TitleScene") {
            Debug.LogWarning("Scene validation for TitleScene failed! (intended behavior)");
            return false;
        }
        Debug.LogWarning($"Scene validation for {sceneName} succeeded!");
        return true;
    }


    private IEnumerator ConnectSceneVerificationEvents() {
        yield return new WaitForSeconds(1f);
        yield return new WaitUntil(() => NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null);
        NetworkManager.Singleton.SceneManager.VerifySceneBeforeLoading -= VerifyScene;
        NetworkManager.Singleton.SceneManager.VerifySceneBeforeLoading += VerifyScene;
        
    }

    public void LoadTitleScene()
    {
        ResubscribeToSceneVerificationEvents();
        Debug.LogWarning("Resubscribed to scene verification events via LocalSceneManager LoadTitleScene");
        // ResubscribeToSceneVerificationEvents();
        Scene existing = SceneManager.GetSceneByName("TitleScene");
        if (!existing.IsValid() || !existing.isLoaded)
        {
            SceneManager.LoadScene("TitleScene", LoadSceneMode.Additive);
        }
    }

    public void UnloadTitleScene()
    {
        Scene titleScene = SceneManager.GetSceneByName("TitleScene");
        if (titleScene.IsValid() && titleScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(titleScene);
        }
    }

    private void OnApplicationQuit() {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null) {
            NetworkManager.Singleton.SceneManager.VerifySceneBeforeLoading -= VerifyScene;
            Debug.LogWarning("Unsubscribed to scene verification events via LocalSceneManager OnApplicationQuit");
        }
    }

}
