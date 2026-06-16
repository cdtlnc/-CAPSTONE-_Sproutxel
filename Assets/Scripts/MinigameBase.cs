using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Identifier enum to pass back to the plot layout
public enum MinigameType
{
    Watering,
    Weeding,
    PestControl,
    SoilEnrichment,
    StructuralSupport,
    Netting 
}

public abstract class MinigameBase : MonoBehaviour
{
    [Header("Minigame Configuration")]
    [SerializeField] protected MinigameType minigameType; // Set this dropdown in the Unity Inspector for each minigame prefab

    [SerializeField] protected TMP_Text resultText;
    [SerializeField] protected TMP_Text instructionText;
    [SerializeField] protected float returnDelay = 2f;
    private Coroutine _returnCoroutine;
    protected bool GameOver { get; private set; } = false;
    public GrowthManager CurrentPlot { get; set; }

    protected virtual void Start()
    {
        // Automatically link this scene's instance to the plot that launched it
        if (MinigameLauncher.activePlot != null)
        {
            CurrentPlot = MinigameLauncher.activePlot;
        }
    }

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

        // Pass the context of which minigame was played back to the plot layout
        if (CurrentPlot != null)
        {
            
            if (won)
            {
                CurrentPlot.ResolveMinigameWin(minigameType);
                AudioManager.instance.Play("WinMinigame");
            }

            else
            {
                CurrentPlot.ResolveMinigameLose(minigameType);
                AudioManager.instance.Play("FailedMinigame");
            }
                
        }

        _returnCoroutine = StartCoroutine(ReturnAfterDelay());
    }

    // Override in each minigame to customize the result messages
    protected virtual string GetWinMessage() => "Success!";
    protected virtual string GetLoseMessage() => "Failed!";

    private IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(returnDelay);

        Debug.Log("Minigame complete - Unloading minigame scene additively.");

        // Clear the active reference tracker safely
        MinigameLauncher.activePlot = null;

        // Unload this specific scene, seamlessly returning the player to the underlying farm scene
        SceneManager.UnloadSceneAsync(gameObject.scene.name);
    }

    public void ResetGame()
    {
        GameOver = false;

        // Stop the scene from unloading if a reset is requested
        if (_returnCoroutine != null)
        {
            StopCoroutine(_returnCoroutine);
            _returnCoroutine = null;
        }
    }
}