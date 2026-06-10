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
        // FIXED: Combines all names in the array into a single string separated by commas (e.g., "Corn, Tomato, Wheat")
        string allCrops = string.Join(", ", targetCropName);

        goalText.text = $"Harvest {currentHarvested}/{targetGoal} {allCrops}";
    }

    public void checkObjectives()
    {
        Debug.Log("CHECKING OBJECTIVESS");
        if (currentHarvested >= targetGoal)
        {
            Debug.Log("SENDING TO THE WIN MANAGER");
            WinOrLoseManager w = FindAnyObjectByType<WinOrLoseManager>();
            w.onWin();
        }
    }
}
