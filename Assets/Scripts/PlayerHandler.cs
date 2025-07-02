using Unity.Netcode;
using UnityEngine;

public class PlayerHandler : NetworkBehaviour
{
    public RPSGameManager gameManager;

    private UIManager uiManager;

    private void Start()
    {
        // Assign only for owner to connect UI
        if (IsOwner)
        {
            gameManager = FindObjectOfType<RPSGameManager>();
            uiManager = FindObjectOfType<UIManager>();
            uiManager.SetPlayer(this);
        }

        // Always assign gameManager even on server side
        if (IsServer && gameManager == null)
        {
            gameManager = FindObjectOfType<RPSGameManager>();
        }
    }

    public void MakeChoice(string choice)
    {
        // Optional: show own choice early
        bool isPlayer1 = IsServer; // Simplified; adjust if needed
        uiManager?.ShowOwnChoice(choice, isPlayer1);

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

        gameManager.RegisterChoice(choice, OwnerClientId);
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
