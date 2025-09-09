using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour {
    [SerializeField] private LocalHostManager localHostManager;
    [SerializeField] private GameObject hostButtonPrefab;
    [SerializeField] private Transform hostListContainer;
    [SerializeField] private Button hostGameButton, singleplayerButton;
    [SerializeField] private float refreshInterval = 1f;
    [SerializeField] private GameObject titleFrame, joinFrame, hostFrame;
    private float _nextRefreshTime;
    private void Start() {
        if (localHostManager != null) {
            hostGameButton.onClick.AddListener(() => {
                localHostManager.StartHostSession();
            });
            singleplayerButton.onClick.AddListener((() => localHostManager.StartHostSession()));
        } else {
            Debug.LogError("LobbyUIManager: Required components are not assigned in the Inspector.", this);
        }
        RefreshUI();
    }
    private void Update() {
        if (Time.time >= _nextRefreshTime) {
            RefreshUI();
            _nextRefreshTime = Time.time + refreshInterval;
        }
    }
    public void ShowJoinGameScreen() {
        Debug.LogWarning("Started listening via LobbyUIManager ShowJoinGameScreen");
        LanDiscovery.Instance.StartListening();
        joinFrame.SetActive(true);
        hostFrame.SetActive(false);
        titleFrame.SetActive(false);
    }
    public void ShowHostGameScreen() {
        hostFrame.SetActive(true);
        joinFrame.SetActive(false);
        titleFrame.SetActive(false);
    }
    public void ReturnToMainMenu() {
        // If was hosting, stop hosting.
        if (NetworkManager.Singleton != null) {
            if (NetworkManager.Singleton.IsHost) {
                Debug.LogWarning("Stopped broadcasting via LobbyUIManager ReturnToMainMenu");
                LanDiscovery.Instance.StopBroadcasting();
                LanDiscovery.Instance.Disconnect();
            } else if (NetworkManager.Singleton.IsClient) {
                Debug.LogWarning("Stopped listening via LobbyUIManager ReturnToMainMenu");
                LanDiscovery.Instance.StopListening();
            }
        }

        // Switch UI back to title
        hostFrame.SetActive(false);
        joinFrame.SetActive(false);
        titleFrame.SetActive(true);
    }
    

    private void RefreshUI() {
        foreach (Transform child in hostListContainer)
            Destroy(child.gameObject);
        var hosts = LanDiscovery.Instance.GetActiveHosts();
        Debug.Log($"Number of active hosts: {hosts.Count}");
        foreach (var host in hosts) {
            string ip = host.ip;
            int port = host.port;
            var btnObj = Instantiate(hostButtonPrefab, hostListContainer);
            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = $"Host @ {ip}:{port}";
            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => { localHostManager.StartClientSession(ip, port); });
        }
    }
}
