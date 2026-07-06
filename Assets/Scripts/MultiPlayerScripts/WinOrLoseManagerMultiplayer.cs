using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinOrLoseManager_Multiplayer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Image resultPanelImage;

    [Header("Panel Colors")]
    [SerializeField] private Color winnerColor = Color.green;
    [SerializeField] private Color loserColor = Color.red;

    private bool gameEnded;

    private void Start()
    {
        gameEnded = false;

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (resultPanelImage != null)
            resultPanelImage.color = loserColor;
    }

    public void ShowWinner()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultText != null)
            resultText.text = "Winner";

        if (resultPanelImage != null)
            resultPanelImage.color = winnerColor;

        AudioManager.instance.Play("WinLevel");

        Debug.Log("[Win/Lose] Winner screen shown.");
    }

    public void ShowLoser()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultText != null)
            resultText.text = "Loser";

        if (resultPanelImage != null)
            resultPanelImage.color = loserColor;

        AudioManager.instance.Play("WinLevel");

        Debug.Log("[Win/Lose] Loser screen shown.");
    }
}