using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RPSGameManager : NetworkBehaviour
{
    private string player1Choice;
    private string player2Choice;

    public ulong Player1ClientId { get; private set; }
    public ulong Player2ClientId { get; private set; }

    private HashSet<ulong> connectedPlayers = new HashSet<ulong>();
    public NetworkVariable<bool> gameStarted = new NetworkVariable<bool>(false);

    public Button startGameButton;
    public GameObject lobbyUI;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        if (startGameButton != null)
        {
            startGameButton.gameObject.SetActive(false);
            startGameButton.onClick.AddListener(() => StartGame());
        }

        UpdateUI();
    }

    private void OnClientConnected(ulong clientId)
    {
        connectedPlayers.Add(clientId);
        Debug.Log($"[GameManager] Player {clientId} connected. Total: {connectedPlayers.Count}");

        // Assign player1 and player2 if not assigned
        if (Player1ClientId == 0)
            Player1ClientId = clientId;
        else if (Player2ClientId == 0 && clientId != Player1ClientId)
            Player2ClientId = clientId;

        UpdateUI();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        connectedPlayers.Remove(clientId);
        Debug.Log($"[GameManager] Player {clientId} disconnected. Total: {connectedPlayers.Count}");

        if (clientId == Player1ClientId) Player1ClientId = 0;
        if (clientId == Player2ClientId) Player2ClientId = 0;

        gameStarted.Value = false;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (startGameButton != null)
            startGameButton.gameObject.SetActive(IsServer && connectedPlayers.Count == 2 && !gameStarted.Value);

        if (lobbyUI != null)
            lobbyUI.SetActive(!gameStarted.Value);
    }

    public void StartGame()
    {
        if (!IsServer) return;

        Debug.Log("[GameManager] Start Game button clicked.");
        gameStarted.Value = true;

        player1Choice = null;
        player2Choice = null;

        UpdateUI();
    }

    public void RegisterChoice(string choice, ulong clientId)
    {
        if (!gameStarted.Value)
        {
            Debug.LogWarning("[GameManager] Game has not started yet.");
            return;
        }

        Debug.Log($"[SERVER] {clientId} chose {choice}");

        if (player1Choice == null && clientId == Player1ClientId)
        {
            player1Choice = choice;
        }
        else if (player2Choice == null && clientId == Player2ClientId)
        {
            player2Choice = choice;
        }
        else
        {
            Debug.LogWarning("[GameManager] Invalid player choice or duplicate.");
            return;
        }

        if (player1Choice != null && player2Choice != null)
        {
            DetermineWinner();
        }
    }

    private void DetermineWinner()
    {
        string resultP1, resultP2;

        if (player1Choice == player2Choice)
        {
            resultP1 = resultP2 = "Draw!";
        }
        else if ((player1Choice == "Rock" && player2Choice == "Scissors") ||
                 (player1Choice == "Paper" && player2Choice == "Rock") ||
                 (player1Choice == "Scissors" && player2Choice == "Paper"))
        {
            resultP1 = "You Win!";
            resultP2 = "You Lose!";
        }
        else
        {
            resultP1 = "You Lose!";
            resultP2 = "You Win!";
        }

        Debug.Log($"[SERVER] P1({player1Choice}) vs P2({player2Choice}) → {resultP1} / {resultP2}");

        ShowResultClientRpc(player1Choice, player2Choice, resultP1, Player1ClientId);
        ShowResultClientRpc(player1Choice, player2Choice, resultP2, Player2ClientId);

        player1Choice = null;
        player2Choice = null;

        gameStarted.Value = false;
        UpdateUI();
    }

    [ClientRpc]
    private void ShowResultClientRpc(string p1Choice, string p2Choice, string result, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            UIManager ui = FindObjectOfType<UIManager>();
            if (ui != null)
            {
                bool isPlayer1 = clientId == Player1ClientId;
                ui.RevealChoices(p1Choice, p2Choice);
                ui.DisplayResult(result);
                ui.SetChoicesInteractable(false);
            }
            else
            {
                Debug.LogWarning("UIManager not found in scene.");
            }
        }
    }
}
