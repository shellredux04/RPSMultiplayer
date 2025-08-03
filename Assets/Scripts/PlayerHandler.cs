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
        }

        if (IsServer && gameManager == null)
        {
            gameManager = FindObjectOfType<RPSGameManager>();
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

        bool isPlayer1 = IsServer;
        uiManager?.ShowOwnChoice(choice, isPlayer1);
    }

    [ClientRpc]
    public void RevealChoicesClientRpc(string p1Choice, string p2Choice, string result)
    {
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();

        uiManager.RevealChoices(p1Choice, p2Choice);
        uiManager.DisplayResult(result);
    }
}
