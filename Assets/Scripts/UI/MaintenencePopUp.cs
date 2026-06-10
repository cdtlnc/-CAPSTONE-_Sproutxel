using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MaintenencePopUp : MonoBehaviour
{
    

    [Header("Game Panels")]
    public GameObject panels; //Just to open up the panels
    public Camera cam; // Maybe to change cam positions when its time
                       // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Plant Stats")]
    [SerializeField] private string name;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private float cropHP;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Sprite plantSprite;
    [SerializeField] private Image plantSpriteShower;

    
    [Header("Crop Moisture")]
    [SerializeField] private float cropMoisture;
    [SerializeField] private Slider cropMoisterSlider;

    [Header("Soil Quality")]
    [SerializeField] private float soilQuality;
    [SerializeField] private Slider soilQualitySlider;

    [Header("Soil Moisture")]
    [SerializeField] private float soilMoisture;
    [SerializeField] private Slider soilMoistureSlider;

    [Header("Soil Softness")]
    [SerializeField] private float soilSoftness;
    [SerializeField] private Slider soilSoftnessSlider;

    [Header("Game Controllers")]
    [Description("To be used when setting parameters on levels or testing")]
    [SerializeField] private bool enStart;

    [Header("Drop Handler")]
    [SerializeField] private PlantViewDrop dropHandler;


    private GrowthManager targetedPlot;
    void Start()
    {
       
        
    }

    // Update is called once per frame
    void Update()
    {
        RefreshUI();
    }

    public void OpenWindow(GrowthManager plot)
    {
        targetedPlot = plot;

        // Pass the plot data to the drop zone so it knows which plant is being worked on
        if (dropHandler != null)
        {
            dropHandler.CurrentPlot = plot;
        }

        if (panels != null) panels.SetActive(true);

        RefreshUI();
    }

    public void CloseWindow()
    {
        targetedPlot = null;

        // Clear out the plot data when the window closes to prevent accidental bugs
        if (dropHandler != null)
        {
            dropHandler.CurrentPlot = null;
        }

        if (panels != null) panels.SetActive(false);
    }

    private void RefreshUI()
    {
        // Safety exit if nothing is selected, panel is closed, or plant was harvested
        if (targetedPlot == null || !targetedPlot.isPlanted || targetedPlot.plantSimulationInstance == null) return;

        var sim = targetedPlot.plantSimulationInstance;

        // Sync variables
        //name=sim.cur // Add in the names
   
        plantSprite = sim.growthStages[3];
        plantSpriteShower.sprite = plantSprite;
        cropHP = sim.cropHP;
        cropMoisture = sim.cropMoisture;
        soilMoisture = sim.soilMoisture;
        soilSoftness = sim.soilSoftness;
        soilQuality = sim.soilQuality;
        nameText.text = targetedPlot.name;

        // Push values directly into sliders

        if (hpSlider != null) hpSlider.value = cropHP;
        if (soilMoistureSlider != null) soilMoistureSlider.value = cropMoisture;
        if (soilQualitySlider != null) soilQualitySlider.value = soilQuality;
        if (soilMoistureSlider != null) soilMoistureSlider.value = soilMoisture;
        if (soilSoftnessSlider != null) soilSoftnessSlider.value = soilSoftness;
        Debug.Log("HpSlider" + hpSlider.value+" "+cropMoisterSlider.value+" "+soilQualitySlider.value+" "+soilMoistureSlider.value+" "+soilSoftnessSlider.value);
    }

  
}
