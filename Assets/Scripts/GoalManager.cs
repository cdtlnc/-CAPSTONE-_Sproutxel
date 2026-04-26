using UnityEngine;
using TMPro;

public class GoalManager : MonoBehaviour
{
    public int eggplantGoal = 10;
    private int eggplantsHarvested = 0;
    public TMP_Text goalText; // Drag your TMP text here

    void Start() { UpdateUI(); }

    public void AddEggplant()// Need to update this with its parameters
    {
        eggplantsHarvested++;
        UpdateUI();
    }

    void UpdateUI()
    {
        goalText.text = $"Goal: {eggplantsHarvested} / {eggplantGoal} Eggplants";
    }
}