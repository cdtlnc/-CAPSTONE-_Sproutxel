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

    /*
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
    */// Removed this for now so that we can focus on making the maintenence screen open.

    private void OnMouseDown()
    {
        MaintenceOpen();

    }
    public void MaintenceOpen()
    {
        Debug.Log("Step 1 Opening Maintenence");
        if (currentStage == 3 && isPlanted == true)
        {
            Debug.Log("Step 2 Opening Panel");
            MaintenencePopUp main = GameObject.FindFirstObjectByType<MaintenencePopUp>();
            if (main != null)
            {
                Debug.Log("Step 3.A not null");
                main.OpenMaintenence();
            }
            else
                Debug.Log("Step 3.B null");

        }
    }

    void ResetPlot()
    {
        isPlanted = false;
        currentStage = 0;
        plantRenderer.sprite = null;
    }
}