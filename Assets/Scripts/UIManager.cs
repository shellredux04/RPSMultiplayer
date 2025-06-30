using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button rockButton;
    public Button paperButton;
    public Button scissorsButton;
    public Text resultText;

    private PlayerHandler player;

    private void Start()
    {
        resultText.text = "";
        rockButton.onClick.AddListener(() => player?.MakeChoice("Rock"));
        paperButton.onClick.AddListener(() => player?.MakeChoice("Paper"));
        scissorsButton.onClick.AddListener(() => player?.MakeChoice("Scissors"));
    }

    public void SetPlayer(PlayerHandler handler)
    {
        player = handler;
    }

    public void DisplayResult(string result)
    {
        resultText.text = result;
    }
}
