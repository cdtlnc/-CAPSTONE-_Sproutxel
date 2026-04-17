using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyItemUI : MonoBehaviour
{
    // TEST SCRIPT FOR LOBBY ITEM UI

    [Header("LobbyUI")]
    [SerializeField] private TMP_Text ownerNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private Button joinButton;

    public void Setup(string ownerName, int players)
    {
        if (ownerName.Length > 9)
        {
            ownerName = ownerName.Substring(0, 11) + "…";
        }

        ownerNameText.text = ownerName + "'s Farm";
        playerCountText.text = players + " / 2 Players";

        joinButton.interactable = players < 2;
    }

    public void OnJoinClicked()
    {
        Debug.Log("Joining lobby...");
    }
}
