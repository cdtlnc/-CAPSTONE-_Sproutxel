using UnityEngine;

public class GrowthManagerOld : MonoBehaviour
{
    public SpriteRenderer plantRenderer;
    private SeedData currentSeed;

    [HideInInspector] public bool isPlanted = false;
    private int currentStage = 0;
    private int ticksElapsed = 0;

    void OnEnable() { TimeManager.OnTick += HandleTick; }
    void OnDisable() { TimeManager.OnTick -= HandleTick; }

    void HandleTick()
    {
        if (!isPlanted || currentSeed == null) return;

        if (currentStage < currentSeed.growthStages.Length - 1)
        {
            ticksElapsed++;
            if (ticksElapsed >= currentSeed.ticksPerStage)
            {
                ticksElapsed = 0;
                currentStage++;
                plantRenderer.sprite = currentSeed.growthStages[currentStage];
            }
        }
    }

    public void PlantSeed(SeedData data)
    {
        if (isPlanted) return;
        currentSeed = data;
        isPlanted = true;
        currentStage = 0;
        ticksElapsed = 0;
        plantRenderer.sprite = currentSeed.growthStages[0];
    }

    void OnMouseDown() // Works for mobile taps
    {
        if (isPlanted && currentStage == currentSeed.growthStages.Length - 1)
        {
            FindObjectOfType<GoalManager>().AddCrop(currentSeed.cropName);
            ResetPlot();
        }
    }

    void ResetPlot()
    {
        isPlanted = false;
        currentSeed = null;
        plantRenderer.sprite = null;
    }
}