using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour {
    [SerializeField] private LocalHostManager localHostManager;
    [SerializeField] private GameObject hostButtonPrefab;
    [SerializeField] private Transform hostListContainer;
    [SerializeField] private Button hostGameButton;
    [SerializeField] private float refreshInterval = 1f;
    [SerializeField] private GameObject titleFrame, joinFrame, hostFrame;
    private float _nextRefreshTime;
    private void Start() {
        if (localHostManager != null && hostGameButton != null) {
            hostGameButton.onClick.AddListener(() => { localHostManager.StartHostSession(); });
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
        // Stop broadcasting if you were the host
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost) {
            LanDiscovery.Instance.StopBroadcasting();
        }
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
            btn.onClick.AddListener(() => {
                localHostManager.StartClientSession(ip, port);
            });

        }
    }
}
