using Unity.Netcode;
using UnityEngine;

public class PlayerHandler : NetworkBehaviour
{
    public RPSGameManager gameManager;

    private void Start()
    {
        if (!IsOwner) return;

        gameManager = FindObjectOfType<RPSGameManager>();
        FindObjectOfType<UIManager>().SetPlayer(this);
    }

    public void MakeChoice(string choice)
    {
        SubmitChoiceServerRpc(choice);
    }

    [ServerRpc]
    private void SubmitChoiceServerRpc(string choice, ServerRpcParams rpcParams = default)
    {
        gameManager.RegisterChoice(choice, OwnerClientId);
    }
}
