using Unity.Netcode;
using UnityEngine;

public class RPSGameManager : NetworkBehaviour
{
    private string player1Choice;
    private string player2Choice;

    private ulong player1Id;
    private ulong player2Id;

    public void RegisterChoice(string choice, ulong clientId)
    {
        if (player1Choice == null)
        {
            player1Choice = choice;
            player1Id = clientId;
        }
        else if (player2Choice == null && clientId != player1Id)
        {
            player2Choice = choice;
            player2Id = clientId;
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

        ShowResultClientRpc(resultP1, player1Id);
        ShowResultClientRpc(resultP2, player2Id);

        player1Choice = player2Choice = null;
    }

    [ClientRpc]
    private void ShowResultClientRpc(string result, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            FindObjectOfType<UIManager>().DisplayResult(result);
        }
    }
}
