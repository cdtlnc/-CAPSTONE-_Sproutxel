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

    public void AddCrop(string cropName)
    {
        
            if (cropName == targetCropName[0])// Need to update to be more dynamic
            {
                currentHarvested++;
                UpdateUI();
            }
        
        
    }

    void UpdateUI() { 
        goalText.text = $"Harvest {currentHarvested}/{targetGoal} {targetCropName}"; 
    }
}