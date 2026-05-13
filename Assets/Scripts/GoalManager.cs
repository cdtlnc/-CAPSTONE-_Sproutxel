using UnityEngine;
using TMPro;

public class GoalManager : MonoBehaviour
{
    public string targetCropName; // e.g., "Corn"
    public int targetGoal;
    private int currentHarvested = 0;
    public TMP_Text goalText;

    void Start() { UpdateUI(); }

    public void AddCrop(string cropName)
    {
        if (cropName == targetCropName)
        {
            currentHarvested++;
            UpdateUI();
        }
    }

    void UpdateUI() { goalText.text = $"Harvest {currentHarvested}/{targetGoal} {targetCropName}"; }
}