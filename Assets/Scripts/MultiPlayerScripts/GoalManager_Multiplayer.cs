using UnityEngine;
using TMPro;

public class GoalManager_Multiplayer : MonoBehaviour
{
    [Header("Goal Settings")]
    [SerializeField] public string[] targetCropName; // e.g., ["Corn", "Tomato", "Wheat"]
    [SerializeField] public int targetGoal;
    [SerializeField] private int currentHarvested = 0;
    [SerializeField] public TMP_Text goalText;

    [Header("Multiplayer Settings")]
    [SerializeField] public string[] targetCropNamss;

    void Start() { UpdateUI(); }

    public void AddCrop(string cropName, int yield)
    {
        foreach (string crop in targetCropName)
        {
            if (cropName == crop)
            {
                FindFirstObjectByType<AudioManager>().Play("Harvest");
                Debug.Log("IM GIVING HOW MUCH?");
                currentHarvested += yield;
                UpdateUI();
                checkObjectives();

                break; // FIXED: Stops the loop immediately after finding a match so it doesn't double-count
            }
        }
    }

    void UpdateUI()
    {
        if (goalText != null && targetCropName != null && targetCropName.Length > 0)
        {
            // Combines all names in the array into a single string separated by commas
            string cropListString = string.Join(", ", targetCropName);
            goalText.text = $"Harvest {cropListString}: {currentHarvested} / {targetGoal}";
        }
    }

    public void checkObjectives()
    {
        Debug.Log("CHECKING OBJECTIVESS");
        if (currentHarvested >= targetGoal)
        {
            Debug.Log("SENDING TO THE WIN MANAGER");

            // Look for the specific multiplayer win/lose script wrapper
            WinOrLoseManager_Multiplayer w = FindAnyObjectByType<WinOrLoseManager_Multiplayer>();
            if (w != null)
            {
                w.onWin(); // Replace or supplement this based on your network victory setup
            }
            else
            {
                // Fallback loop check for singleplayer setups
                WinOrLoseManager fallbackWin = FindAnyObjectByType<WinOrLoseManager>();
                if (fallbackWin != null) fallbackWin.onWin();
            }
        }
    }

    public void LoseGame(string loser)
    {
        Debug.Log("[STEP 6] LOST THE GAME Loser: " + loser);
        WinOrLoseManager_Multiplayer w = FindAnyObjectByType<WinOrLoseManager_Multiplayer>();
        if (w != null)
        {
            w.onLose(loser);
        }
        else
        {
            Debug.LogError("WinOrLoseManager_Multiplayer missing from the active map hierarchy!");
        }
    }
}