using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class LobbyManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField joinCodeInput;
    public TextMeshProUGUI lobbyCodeText;
    public GameObject lobbyUI;
    public GameObject connectingBuffer;
    public GameObject gameStartButton;

    private UnityTransport transport;
    private string joinCode = "";

    private async void Start()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (joinCodeInput != null)
        {
            joinCodeInput.onValueChanged.AddListener(OnJoinCodeInputChanged);
        }

        await InitializeUnityServices();

        // Initial UI state
        ShowLobbyUI();
    }

    private void OnJoinCodeInputChanged(string input)
    {
        joinCode = input.Trim();
    }

    public void Host()
    {
        Debug.Log("[LobbyManager] Host() called.");
        _ = HostRelayAsync(4);
    }

    public void Join()
    {
        Debug.Log("[LobbyManager] Join() called.");

        if (!string.IsNullOrEmpty(joinCode))
        {
            _ = JoinRelayAsync(joinCode);
        }
        else
        {
            Debug.LogWarning("[LobbyManager] Join code is empty.");
        }
    }

    private async Task HostRelayAsync(int maxPlayers)
    {
        ShowConnectingBuffer();
        Debug.Log("[LobbyManager] Starting Relay host...");

        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            string code = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"[Relay] Join Code: {code}");

            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            bool success = NetworkManager.Singleton.StartHost();

            if (success)
            {
                Debug.Log("[LobbyManager] Host started successfully.");
                lobbyCodeText.text = $"Lobby Code: {code}";
                lobbyCodeText.gameObject.SetActive(true);
                gameStartButton.SetActive(true);
                ShowLobbyUI();
            }
            else
            {
                Debug.LogError("[LobbyManager] Host failed to start.");
                ShowLobbyUI();
            }
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"[LobbyManager] Relay host error: {e.Message}");
            ShowLobbyUI();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LobbyManager] Unknown host error: {e.Message}");
            ShowLobbyUI();
        }
    }

    private async Task JoinRelayAsync(string code)
    {
        ShowConnectingBuffer();
        Debug.Log($"[LobbyManager] Joining Relay with code: {code}");

        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);

            transport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            bool success = NetworkManager.Singleton.StartClient();

            if (success)
            {
                Debug.Log("[LobbyManager] Client joined successfully.");
                lobbyCodeText.text = $"Joined Lobby: {code}";
                gameStartButton.SetActive(false);
                ShowLobbyUI();
            }
            else
            {
                Debug.LogError("[LobbyManager] Client failed to connect.");
                ShowLobbyUI();
            }
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"[LobbyManager] Relay join error: {e.Message}");
            ShowLobbyUI();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LobbyManager] Unknown join error: {e.Message}");
            ShowLobbyUI();
        }
    }

    private async Task InitializeUnityServices()
    {
        Debug.Log("[LobbyManager] Initializing Unity Services...");

        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("[LobbyManager] Signed in anonymously.");
        }
    }

    private void ShowLobbyUI()
    {
        if (connectingBuffer != null) connectingBuffer.SetActive(false);
        if (lobbyUI != null) lobbyUI.SetActive(true);
    }

    private void ShowConnectingBuffer()
    {
        if (connectingBuffer != null) connectingBuffer.SetActive(true);
        if (lobbyUI != null) lobbyUI.SetActive(false);
    }
}
