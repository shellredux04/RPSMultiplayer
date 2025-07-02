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
        Debug.Log($"[SERVER] {clientId} chose {choice}");

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

        // Determine who wins
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

        // Reveal both choices and results to each player
        ShowResultClientRpc(player1Choice, player2Choice, resultP1, player1Id);
        ShowResultClientRpc(player1Choice, player2Choice, resultP2, player2Id);

        // Reset for next round
        player1Choice = null;
        player2Choice = null;
    }

    [ClientRpc]
    private void ShowResultClientRpc(string p1Choice, string p2Choice, string result, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            UIManager ui = FindObjectOfType<UIManager>();
            if (ui != null)
            {
                ui.RevealChoices(p1Choice, p2Choice);
                ui.DisplayResult(result);
            }
            else
            {
                Debug.LogWarning("UIManager not found in scene.");
            }
        }
    }
}
