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
        Debug.Log("LobbyManager Start() running");

        // Ensure UnityTransport is attached
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (joinCodeInput != null)
        {
            joinCodeInput.onValueChanged.AddListener(OnJoinCodeInputChanged);
            Debug.Log("Join code input listener added.");
        }
        else
        {
            Debug.LogWarning("joinCodeInput is not assigned in Inspector.");
        }

        await InitializeUnityServices();
        Debug.Log("Unity Services Initialized.");

        ShowLobbyUI();
    }

    private void OnJoinCodeInputChanged(string input)
    {
        joinCode = input.Trim();
    }

    public void Host()
    {
        Debug.Log("Host button clicked");
        _ = HostRelayAsync(4);
    }

    public void Join()
    {
        Debug.Log($"Join button clicked. Code: {joinCode}");

        if (!string.IsNullOrEmpty(joinCode))
        {
            _ = JoinRelayAsync(joinCode);
        }
        else
        {
            Debug.LogWarning("Join code is empty.");
        }
    }

    private async Task HostRelayAsync(int maxPlayers)
    {
        ShowConnectingBuffer();

        try
        {
            Debug.Log("Creating Relay allocation...");
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            string code = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

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
                Debug.Log("Host started successfully.");
                lobbyCodeText.text = $"Lobby Code: {code}";
                gameStartButton.SetActive(true);
                ShowLobbyUI();
            }
            else
            {
                Debug.LogError("Host failed to start.");
                ShowLobbyUI();
            }
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay host error: {e.Message}");
            ShowLobbyUI();
        }
    }

    private async Task JoinRelayAsync(string code)
    {
        ShowConnectingBuffer();

        try
        {
            Debug.Log("Joining Relay allocation...");
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
                Debug.Log("Client joined successfully.");
                lobbyCodeText.text = $"Joined Lobby: {code}";
                gameStartButton.SetActive(false);
                ShowLobbyUI();
            }
            else
            {
                Debug.LogError("Client failed to connect.");
                ShowLobbyUI();
            }
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay join error: {e.Message}");
            ShowLobbyUI();
        }
    }

    private async Task InitializeUnityServices()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            Debug.Log("Initializing Unity Services...");
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Signing in anonymously...");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private void ShowLobbyUI()
    {
        Debug.Log("Showing Lobby UI.");
        connectingBuffer?.SetActive(false);
        lobbyUI?.SetActive(true);
    }

    private void ShowConnectingBuffer()
    {
        Debug.Log("Showing connecting buffer.");
        connectingBuffer?.SetActive(true);
        lobbyUI?.SetActive(false);
    }
}
