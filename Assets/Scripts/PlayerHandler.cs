using Unity.Netcode;
using UnityEngine;

public class PlayerHandler : NetworkBehaviour
{
    public RPSGameManager gameManager;
    private UIManager uiManager;

    private void Start()
    {
        if (IsOwner)
        {
            gameManager = FindObjectOfType<RPSGameManager>();
            uiManager = FindObjectOfType<UIManager>();
            uiManager.SetPlayer(this);
            uiManager.SetChoicesInteractable(false);  // Start disabled until game starts
        }

        if (IsServer && gameManager == null)
        {
            gameManager = FindObjectOfType<RPSGameManager>();
        }

        // Listen for game started changes
        if (gameManager != null)
            gameManager.gameStarted.OnValueChanged += OnGameStartedChanged;
    }

    private void OnDestroy()
    {
        if (gameManager != null)
            gameManager.gameStarted.OnValueChanged -= OnGameStartedChanged;
    }

    private void OnGameStartedChanged(bool previous, bool current)
    {
        if (!IsOwner) return;

        uiManager.SetChoicesInteractable(current);

        if (!current)
        {
            // Clear UI choices and result on game end
            uiManager.ClearChoices();
            uiManager.ClearResult();
        }
    }

    public void MakeChoice(string choice)
    {
        SubmitChoiceServerRpc(choice);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitChoiceServerRpc(string choice, ServerRpcParams rpcParams = default)
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<RPSGameManager>();
            if (gameManager == null)
            {
                Debug.LogError("SubmitChoiceServerRpc failed: RPSGameManager not found.");
                return;
            }
        }

        // Send choice back to the client to display hand early
        ShowOwnChoiceClientRpc(choice, OwnerClientId);

        gameManager.RegisterChoice(choice, OwnerClientId);
    }

    [ClientRpc]
    public void ShowOwnChoiceClientRpc(string choice, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId)
            return;

        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();

        bool isPlayer1 = false;

        if (gameManager != null)
        {
            isPlayer1 = OwnerClientId == gameManager.Player1ClientId;
        }

        uiManager?.ShowOwnChoice(choice, isPlayer1);
    }
}
