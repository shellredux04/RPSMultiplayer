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
    private string joinCode;
    public TMP_InputField joinCodeInput;

    public TextMeshProUGUI lobbyCodeText;
    public GameObject lobbyUI;
    public GameObject connectingBuffer;
    public GameObject gameStartButton;

    private UnityTransport transport;

    private void Start()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        joinCodeInput.onValueChanged.AddListener(OnJoinCodeInputChanged);

        // Initial UI setup
        lobbyUI.SetActive(true);
        connectingBuffer.SetActive(false);
        gameStartButton.SetActive(false);
    }

    private void OnJoinCodeInputChanged(string input)
    {
        joinCode = input.Trim();
    }

    public void Host() => _ = HostRelayAsync(4);
    public void Join() => _ = JoinRelayAsync(joinCode);

    private async Task HostRelayAsync(int maxPlayers)
    {
        connectingBuffer.SetActive(true);

        await InitializeUnityServices();

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
        string code = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        transport.SetRelayServerData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData
        );

        bool started = NetworkManager.Singleton.StartHost();
        if (started)
        {
            lobbyCodeText.text = $"Lobby Code: {code}";
            lobbyUI.SetActive(false);
            gameStartButton.SetActive(true);
            Debug.Log($"Hosting with join code: {code}");
        }
        else
        {
            Debug.LogError("Failed to start host.");
            connectingBuffer.SetActive(false);
        }

        connectingBuffer.SetActive(false);
    }

    private async Task JoinRelayAsync(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            Debug.LogError("Join code cannot be empty.");
            return;
        }

        connectingBuffer.SetActive(true);

        await InitializeUnityServices();

        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);

        transport.SetRelayServerData(
            joinAllocation.RelayServer.IpV4,
            (ushort)joinAllocation.RelayServer.Port,
            joinAllocation.AllocationIdBytes,
            joinAllocation.Key,
            joinAllocation.ConnectionData,
            joinAllocation.HostConnectionData
        );

        bool started = NetworkManager.Singleton.StartClient();
        if (started)
        {
            lobbyCodeText.text = $"Joined Lobby: {code}";
            lobbyUI.SetActive(false);
            gameStartButton.SetActive(false);
            Debug.Log($"Joined lobby with code: {code}");
        }
        else
        {
            Debug.LogError("Failed to start client.");
            connectingBuffer.SetActive(false);
        }

        connectingBuffer.SetActive(false);
    }

    private async Task InitializeUnityServices()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
            await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
}
