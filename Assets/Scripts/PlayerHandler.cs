using Unity.Netcode;
using UnityEngine;

public class PlayerHandler : NetworkBehaviour
{
    public RPSGameManager gameManager;

    private void Start()
    {
        // Assign only for owner to connect UI
        if (IsOwner)
        {
            gameManager = FindObjectOfType<RPSGameManager>();
            FindObjectOfType<UIManager>().SetPlayer(this);
        }

        // Always assign gameManager even on server side
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

        gameManager.RegisterChoice(choice, OwnerClientId);
    }
}
