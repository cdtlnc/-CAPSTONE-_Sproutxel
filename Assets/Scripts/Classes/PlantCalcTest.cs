using UnityEngine;

public class PlantCalcTest : MonoBehaviour
{
    // This script is solely meant for testing. It outputs the results of the plant's calculations each frame.
    // This is a sample "listener" script, a script which takes the signals from the TimeOfDayUI.cs and EventManager.cs script and passes it to the plant.
    // Specifically, it takes the signals of what season it is, what time of day it is, and whether or not a bug infestation happens, then it passes these signals to the plant so it can set its resistances accordingly.
    // Right now, the code is set up for only one plant, since this is just for testing. However, in actual implementation, there will be a list that stores each plant actively growing, and for each tick, the code will
    // loop through every plant stored in that list and perform all these calculations.
    // However, I will leave that up to you guys to implement. I've tried my best to set up the systems and make them as easy as possible to integrate. I hope it works out!

    int seasonIndex;
    int cycleIndex;
    int weatherIndex;
    int bugIndex;

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

        TickManager.OnPlantCalcTick += delegate (object sender, TickManager.OnTickEventArgs e)
        {
            Debug.Log("Tick: " + e.tick);
        };

        TickManager.OnPlantCalcTick += TickManager_PlantCalcTick;
    }

    void Update()
    {
        if (TimeOfDayUI.isDrySeason)
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

        if (TimeOfDayUI.isDay)
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

        switch (EventManager._weatherEvent)
        {
            case 0:
                weatherOutput = "CLEAR";
                weatherIndex = 0;
                sample.weatherIndex = weatherIndex;
                break;
            case 1:
                weatherOutput = "HEAT WAVE";
                weatherIndex = 1;
                sample.weatherIndex = weatherIndex;    
                break;
            case 2:
                weatherOutput = "TYPHOON";
                weatherIndex = 2;
                sample.weatherIndex = weatherIndex;
                break;
        }

        if (EventManager.isInfested)
        {
            bugIndex = 1;
            sample.bugIndex = bugIndex;
        }
        else
        {
            bugIndex = 0;
            sample.bugIndex = bugIndex;
        }
    }

    private void TickManager_PlantCalcTick(object sender, TickManager.OnTickEventArgs e)
    {
        sample.GetStatsOvertime();
        sample.GetSoilQuality();
        sample.GetHealth();
        sample.GetGrowth();
        sample.GetHarvestQuality();

        Debug.Log("Season: "          + seasonOutput  + " | Resistance: " + stats.seasonalAffinities[seasonIndex] + "\n"
                  + "Day: "           + cycleOutput   + " | Resistance: " + stats.cycleAffinities[cycleIndex]     + "\n"
                  + "Weather Event: " + weatherOutput + " | Resistance: " + stats.weatherAffinities[weatherIndex] + "\n"
                  + "Infested: "      + bugIndex      + " | Resistance: " + stats.bugResistances[bugIndex] + "\n"

                  + "Crop HP: "         + sample.cropHP         + "\n"
                  + "Crop Moisture: "   + sample.cropMoisture   + "\n"
                  + "Crop Growth: "     + sample.cropGrowth     + "\n"
                  + "Soil Quality: "    + sample.soilQuality    + "\n"
                  + "Soil Moisture: "   + sample.soilMoisture   + "\n"
                  + "Soil Softness: "   + sample.soilSoftness   + "\n"
                  + "Harvest Quality: " + sample.harvestQuality + "\n"
                  + "Crop Yield: "      + sample.GetCropYield()
                 );
    }
}
