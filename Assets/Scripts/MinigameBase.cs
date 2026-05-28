using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public abstract class MinigameBase : MonoBehaviour
{
    [SerializeField] protected TMP_Text resultText;
    [SerializeField] protected TMP_Text instructionText;
    [SerializeField] protected float returnDelay = 2f;

    protected bool GameOver { get; private set; } = false;
    public GrowthManager CurrentPlot { get; set; }
    protected void EndGame(bool won)
    {
        if (GameOver) return;
        GameOver = true;

        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = won ? GetWinMessage() : GetLoseMessage();
        }

        if (instructionText != null)
            instructionText.gameObject.SetActive(false);

        StartCoroutine(ReturnAfterDelay());
    }

    // Override in each minigame to customize the result messages
    protected virtual string GetWinMessage() => "Success!";
    protected virtual string GetLoseMessage() => "Failed!";

    private IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(returnDelay);
        Debug.Log("Minigame complete - return not configured yet.");
    }
}
