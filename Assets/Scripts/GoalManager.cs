using UnityEngine;
using TMPro;

public class GoalManager : MonoBehaviour
{

    [Header("Goal Settings")]
    [SerializeField]  public string[] targetCropName; // e.g., "Corn"
    [SerializeField] public int targetGoal;
    [SerializeField] private int currentHarvested = 0;
    [SerializeField] public TMP_Text goalText;

    void Start() { UpdateUI(); }

    public void AddCrop(string cropName, int yield)
    {
        foreach (string crop in targetCropName)
        {
            if (cropName == crop)// Need to update to be more dynamic
            {
                FindFirstObjectByType<AudioManager>().Play("Harvest");
                Debug.Log("IM GIVING HOW MUCH?");
                currentHarvested += yield;
                UpdateUI();
                checkObjectives();
            }
        }
        
        
    }

    void UpdateUI() { 
        goalText.text = $"Harvest {currentHarvested}/{targetGoal} {targetCropName[0]}"; 
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