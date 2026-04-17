using UnityEngine;

public class GrowthManager : MonoBehaviour
{
    public SpriteRenderer plantRenderer;
    public Sprite[] eggplantStages; // Put your 4 sprites here (0=Seed, 3=Mature)

    [HideInInspector] public bool isPlanted = false;
    private int currentStage = 0;
    private float timer = 0;
    private float timeToGrow = 3f; // 3 seconds per stage

    void Update()
    {
        if (isPlanted && currentStage < 3)
        {
            timer += Time.deltaTime;
            if (timer >= timeToGrow)
            {
                timer = 0;
                currentStage++;
                plantRenderer.sprite = eggplantStages[currentStage];
            }
        }
    }

    public void PlantSeed()
    {
        if (isPlanted) return;
        isPlanted = true;
        currentStage = 0;
        timer = 0;
        plantRenderer.sprite = eggplantStages[0];
    }

    // THIS IS THE HARVEST LOGIC
    void OnMouseDown()
    {
        if (isPlanted && currentStage == 3) // Only harvest if fully grown
        {
            // Tell the GoalManager we got one!
            Object.FindAnyObjectByType<GoalManager>().AddEggplant();

            // Clear the plot
            ResetPlot();
        }
    }

    void ResetPlot()
    {
        isPlanted = false;
        currentStage = 0;
        plantRenderer.sprite = null;
    }
}