using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Choice Buttons")]
    public Button rockButton;
    public Button paperButton;
    public Button scissorsButton;

    [Header("UI Elements")]
    public TMPro.TextMeshProUGUI resultText;
    public Image player1ChoiceImage;
    public Image player2ChoiceImage;
    public Button quitButton;

    [Header("Choice Sprites")]
    public Sprite rockSprite;
    public Sprite paperSprite;
    public Sprite scissorsSprite;

    private PlayerHandler player;

    private Dictionary<string, Sprite> choiceSprites;

    private void Start()
    {
        resultText.text = "";

        rockButton.onClick.AddListener(() => player?.MakeChoice("Rock"));
        paperButton.onClick.AddListener(() => player?.MakeChoice("Paper"));
        scissorsButton.onClick.AddListener(() => player?.MakeChoice("Scissors"));
        quitButton.onClick.AddListener(QuitGame);

        choiceSprites = new Dictionary<string, Sprite>
        {
            {"Rock", rockSprite},
            {"Paper", paperSprite},
            {"Scissors", scissorsSprite}
        };

        player1ChoiceImage.gameObject.SetActive(false);
        player2ChoiceImage.gameObject.SetActive(false);

        SetChoicesInteractable(false);
    }

    public void SetPlayer(PlayerHandler handler)
    {
        player = handler;
    }

    public void SetChoicesInteractable(bool interactable)
    {
        rockButton.interactable = interactable;
        paperButton.interactable = interactable;
        scissorsButton.interactable = interactable;
    }

    public void ShowOwnChoice(string choice, bool isPlayer1)
    {
        Image target = isPlayer1 ? player1ChoiceImage : player2ChoiceImage;
        target.sprite = choiceSprites[choice];
        target.gameObject.SetActive(true);
    }

    public void RevealChoices(string p1Choice, string p2Choice)
    {
        player1ChoiceImage.sprite = choiceSprites[p1Choice];
        player2ChoiceImage.sprite = choiceSprites[p2Choice];

        player1ChoiceImage.gameObject.SetActive(true);
        player2ChoiceImage.gameObject.SetActive(true);
    }

    public void DisplayResult(string result)
    {
        resultText.text = result;
    }

    public void ClearChoices()
    {
        player1ChoiceImage.gameObject.SetActive(false);
        player2ChoiceImage.gameObject.SetActive(false);
    }

    public void ClearResult()
    {
        resultText.text = "";
    }

    public void QuitGame()
    {
        Debug.Log("Quit button pressed.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
