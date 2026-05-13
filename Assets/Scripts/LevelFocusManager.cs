using UnityEngine;
using System.Collections.Generic;

public class LevelFocusManager : MonoBehaviour
{
    [Header("Level Config")]
    public int currentLevel = 1; // Set this to 1 for Level 1 scene, 2 for Level 2, etc.

    [Header("Scene References")]
    public List<GameObject> hotbarSlots; // Drag your 10 UI buttons here
    public List<GameObject> farmPlots;   // Drag your 12 Plot objects from the hierarchy here

    void Start()
    {
        ApplyLevelRules();
    }

    public void ApplyLevelRules()
    {
        // 1. Difficulty Curve: Plot Availability
        // Level 1-3: 3 plots | Level 4-6: 6 plots | Level 7-10: 9 plots
        int plotsToEnable = currentLevel <= 3 ? 3 : (currentLevel <= 6 ? 6 : 9);

        for (int i = 0; i < farmPlots.Count; i++)
        {
            // Only turn on the plots required for this level
            farmPlots[i].SetActive(i < plotsToEnable);
        }

        // 2. Difficulty Curve: Available Seeds
        // Level 1: 1 seed | Level 10: 10 seeds
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            hotbarSlots[i].SetActive(i < currentLevel);
        }

        Debug.Log($"Level {currentLevel} Rules Applied: {plotsToEnable} plots active.");
    }
}