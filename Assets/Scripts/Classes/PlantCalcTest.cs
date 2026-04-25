using UnityEngine;

public class PlantCalcTest : MonoBehaviour
{
    // This script is solely meant for testing. It outputs the results of the plant's calculations each frame.

    public bool isHotSeason; 
    public bool isDaytime;

    int seasonIndex;
    int cycleIndex;
    public int weatherIndex;

    string seasonOutput;
    string cycleOutput;
    string weatherOutput;

    public BasePlant sample = new BasePlant();
    public PlantBase stats;

    void Start()
    {
        sample.stats = stats;
        sample.cropHP = stats.maxHP;
        sample.cropMoisture = 50f;
        sample.soilQuality = 100f;
        sample.soilMoisture = 50f;
        sample.soilSoftness = 50f;
    }

    void Update()
    {
        if (isHotSeason)
        {
            seasonOutput = "DRY SEASON";
            seasonIndex = 0;
            sample.seasonIndex = seasonIndex;
        }
        else
        {
            seasonOutput = "WET SEASON";
            seasonIndex = 1;
            sample.seasonIndex = seasonIndex;
        }

        if (isDaytime)
        {
            cycleOutput = "DAY";
            cycleIndex = 0;
            sample.dayIndex = cycleIndex;
        }
        else
        {
            cycleOutput = "NIGHT";
            cycleIndex = 1;
            sample.dayIndex = cycleIndex;
        }

        switch (weatherIndex)
        {
            case 0:
                weatherOutput = "CLEAR";
                sample.weatherIndex = weatherIndex;
                break;
            case 1:
                weatherOutput = "HEAT WAVE";
                sample.weatherIndex = weatherIndex;    
                break;
            case 2:
                weatherOutput = "TYPHOON";
                sample.weatherIndex = weatherIndex;
                break;
        }

        sample.GetStatsOvertime();
        sample.GetSoilQuality();
        sample.GetHealth();
        sample.GetGrowth();
        sample.GetHarvestQuality();

        Debug.Log("Season: "          + seasonOutput  + " | " + "Resistance: " + stats.seasonalAffinities[seasonIndex] + "\n"
                  + "Day: "           + cycleOutput   + " | " + "Resistance: " + stats.cycleAffinities[cycleIndex]     + "\n"
                  + "Weather Event: " + weatherOutput + " | " + "Resistance: " + stats.weatherAffinities[weatherIndex] + "\n"

                  + "Crop HP: "         + sample.cropHP         + "\n"
                  + "Crop Moisture: "   + sample.cropMoisture   + "\n"
                  + "Crop Growth: "     + sample.cropGrowth     + "\n"
                  + "Soil Quality: "    + sample.soilQuality    + "\n"
                  + "Soil Moisture: "   + sample.soilMoisture   + "\n"
                  + "Soil Softness: "   + sample.soilSoftness   + "\n"
                  + "Harvest Quality: " + sample.harvestQuality + "\n"
                  + "Crop Yield: "      + sample.GetCropYield());
    }
}
