using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LocalSceneManager : MonoBehaviour
{
    public static LocalSceneManager Instance;
    

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
        StartCoroutine(ConnectSceneVerificationEvents());
    }


    private bool VerifyScene(int sceneIndex, string sceneName, LoadSceneMode loadSceneMode) {
        if (sceneName == "TitleScene") {
            return false;
        }
        return true;
    }


    private IEnumerator ConnectSceneVerificationEvents() {
        yield return new WaitUntil(() => NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null);
        NetworkManager.Singleton.SceneManager.VerifySceneBeforeLoading += VerifyScene;
    }

    public void LoadTitleScene()
    {
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

    private void OnDestroy() {
        if (NetworkManager.Singleton != null) {
            NetworkManager.Singleton.SceneManager.VerifySceneBeforeLoading -= VerifyScene;
        }
    }

}
