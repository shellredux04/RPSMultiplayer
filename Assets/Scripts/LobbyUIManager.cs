using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class LobbyUIManager : MonoBehaviour
{
    public TMP_InputField nameInput; 
    public Button redButton, blueButton, greenButton;

    void Start()
    {
        redButton.onClick.AddListener(() => SetColor("Red"));
        blueButton.onClick.AddListener(() => SetColor("Blue"));
        greenButton.onClick.AddListener(() => SetColor("Green"));

        nameInput.onEndEdit.AddListener(SetName); 
    }

    void SetColor(string color)
    {
        PlayerInfo.PlayerColor = color;
        Debug.Log("Color set to: " + color);
    }

    void SetName(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            PlayerInfo.PlayerName = name;
            Debug.Log("Name set to: " + name);
        }
    }
}
