using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class LobbyManager : NetworkBehaviour
{
    [Header("UI References")]
    public TMP_InputField joinCodeInput;
    public TextMeshProUGUI lobbyCodeText;
    public GameObject lobbyUI;
    public GameObject connectingBuffer;
    public GameObject gameStartButton;

    [Header("Countdown")]
    public TextMeshProUGUI countdownText;

    private UnityTransport transport;
    private string joinCode = "";

    public NetworkVariable<bool> gameStarted = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private void Awake()
    {
        gameStarted.OnValueChanged += OnGameStartedChanged;
    }

    private async void Start()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (joinCodeInput != null)
            joinCodeInput.onValueChanged.AddListener(OnJoinCodeInputChanged);

        await InitializeUnityServices();

        ShowLobbyUI();

        // Hide countdown text at start
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    private void OnJoinCodeInputChanged(string input)
    {
        joinCode = input.Trim();
    }

    public void Host()
    {
        _ = HostRelayAsync(4);
    }

    public void Join()
    {
        if (!string.IsNullOrEmpty(joinCode))
            _ = JoinRelayAsync(joinCode);
    }

    // Called by the Start button in the lobby UI (only host calls this)
    public void StartGame()
    {
        if (IsServer)
        {
            StartCoroutine(StartGameWithCountdown());
        }
    }

    private IEnumerator StartGameWithCountdown()
    {
        ShowCountdownClientRpc();

        int count = 3;

        while (count > 0)
        {
            UpdateCountdownClientRpc(count.ToString());
            yield return new WaitForSeconds(1f);
            count--;
        }

        UpdateCountdownClientRpc("GO!");
        yield return new WaitForSeconds(1f);

        HideCountdownClientRpc();

        // Now officially start the game
        gameStarted.Value = true;
    }

    private void OnGameStartedChanged(bool previous, bool current)
    {
        if (current)
        {
            Debug.Log("[LobbyManager] Game started! Hiding lobby UI.");
            if (lobbyUI != null)
                lobbyUI.SetActive(false);
        }
    }

    public bool IsGameStarted()
    {
        return gameStarted.Value;
    }

    [ClientRpc]
    private void ShowCountdownClientRpc()
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(true);
    }

    [ClientRpc]
    private void UpdateCountdownClientRpc(string text)
    {
        if (countdownText != null)
            countdownText.text = text;
    }

    [ClientRpc]
    private void HideCountdownClientRpc()
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    private async Task HostRelayAsync(int maxPlayers)
    {
        ShowConnectingBuffer();

        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            string code = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            if (NetworkManager.Singleton.StartHost())
            {
                lobbyCodeText.text = $"Lobby Code: {code}";
                lobbyCodeText.gameObject.SetActive(true);
                gameStartButton.SetActive(true);
                ShowLobbyUI();
            }
            else
            {
                ShowLobbyUI();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LobbyManager] Relay Host Error: {e.Message}");
            ShowLobbyUI();
        }
    }

    private async Task JoinRelayAsync(string code)
    {
        ShowConnectingBuffer();

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

            if (NetworkManager.Singleton.StartClient())
            {
                lobbyCodeText.text = $"Joined Lobby: {code}";
                gameStartButton.SetActive(false);
                ShowLobbyUI();
            }
            else
            {
                ShowLobbyUI();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LobbyManager] Relay Join Error: {e.Message}");
            ShowLobbyUI();
        }
    }

    private async Task InitializeUnityServices()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
            await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
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
