using UnityEngine;
using Sproutxel.MathUtilities;

public class BasePlant
{
    public PlantBase stats;
    public Sprite[] growthStages; // Put your 4 sprites here (0 = Seed, 3 = Mature)

    public float cropHP;         // 0 to crop type's max HP
    public float cropMoisture;   // -100 to 100, healthy range is -20 to 50
    public float cropGrowth = 0; // 0 to 10

    public float soilQuality;  // -100 to 100
    public float soilMoisture; // -100 to 100, healthy range is -20 to 50
    public float soilSoftness; // -100 to 100, healthy range is -20 to 50

    public float harvestQuality; // 0 to 100

    public int seasonIndex { private get; set; }  // This is either 0 or 1. 0 - Dry, 1 - Wet
    public int dayIndex { private get; set; }     // This is either 0 or 1. 0 - Day, 1 - Night
    public int weatherIndex { private get; set; } // This is either 0, 1 or 2. 0 - No weather event, 1 - Heat Wave, 2 - Typhoon

    // These values manage how much these stats change per tick by default. These are NOT external modifiers. Be careful when tinkering with them.
    [SerializeField] private float _soilQualityMultiplier    = 2.3f; // 2.3f by default
    [SerializeField] private float _cropHealthMultiplier     = 0.6f; // 0.6f by default
    [SerializeField] private float _harvestQualityMultiplier = 1.9f; // 1.9f by default

    public void GetStatsOvertime() // This calculates the depreciation of the cropMoisture, soilMoisture and soilSoftness stats
    {
        float cropMoistureMultiplier = 1; // This is set to 1 if we want the stat to go up, -1 if we want it to go down
        float soilMoistureMultiplier = 1; // This is set to 1 if we want the stat to go up, -1 if we want it to go down
        float soilSoftnessMultiplier = 1; // This is set to 1 if we want the stat to go up, -1 if we want it to go down

        // The following if-else statements affect the changes in the following stats depending on the season and the weather.
        // Setting it to -1.00 / 1.00 makes the stat go down and up respectively.
        // Setting it below 1 makes the change slower. I recommend keeping it between 0 and 1.
        // Setting it above 1 makes it faster. I recommend keeping it between 1 and 2.
        // Tinker with the values to achieve the rate of change that you want.
        if (seasonIndex == 0)
        {
            cropMoistureMultiplier = -1;
            soilMoistureMultiplier = -1;
            soilSoftnessMultiplier = 1.25f;
        }
        else if (seasonIndex == 1)
        {
            cropMoistureMultiplier = 1.25f;
            soilMoistureMultiplier = 1.25f;
            soilSoftnessMultiplier = -1;
        }

        if (weatherIndex == 1)
        {
            cropMoistureMultiplier = -1.35f;
            soilMoistureMultiplier = -1.35f;
            soilSoftnessMultiplier = 1.35f;
        }
        else if (weatherIndex == 2)
        {
            cropMoistureMultiplier = -1.35f;
            soilMoistureMultiplier = -1.35f;
            soilSoftnessMultiplier = 1.35f;

        }

        cropMoisture += 2 * (100 + cropMoisture) / 100 * cropMoistureMultiplier;
        soilMoisture += 2 * (100 + soilMoisture) / 100 * soilMoistureMultiplier;
        soilSoftness += 2 * (100 + soilSoftness) / 100 * soilSoftnessMultiplier;

        cropMoisture = Mathf.Clamp(cropMoisture, -100f, 100f);
        soilMoisture = Mathf.Clamp(soilMoisture, -100f, 100f);
        soilSoftness = Mathf.Clamp(soilSoftness, -100f, 100f);
    }

    public void GetSoilQuality() // This uses soilMoisture and soilSoftness to calculate soilQuality
    {
        float soilMoistureBonus = BellCurve.GetFactor(soilMoisture, 15, 20) * 50f;
        float soilSoftnessBonus = BellCurve.GetFactor(soilSoftness, 15, 20) * 50f;

        float rawSoilQualityChange = _soilQualityMultiplier * (soilMoistureBonus + soilSoftnessBonus) / 50;

        soilQuality += (rawSoilQualityChange / 100) * 100;
        soilQuality = Mathf.Clamp(soilQuality, -100f, 100f);
    }

    public void GetHealth() // This uses soilQuality, soilMoisture, soilSoftness and cropMoisture to get cropHP
    {
        if (stats.maxHP <= 0)
        {
            return;
        }

        if (cropHP <= 0)
        {
            return;
        }

        float soilQualityBonus  = (soilQuality / 100) * 25;
        float soilMoistureBonus = BellCurve.GetFactor(soilMoisture, 15, 20) * 25f;
        float soilSoftnessBonus = BellCurve.GetFactor(soilSoftness, 15, 20) * 25f;
        float cropMoistureBonus = BellCurve.GetFactor(cropMoisture, 15, 20) * 25f;

        float rawHealthChange = _cropHealthMultiplier * (soilQualityBonus + soilMoistureBonus + soilSoftnessBonus + cropMoistureBonus) / 100;

        cropHP += rawHealthChange * stats.seasonalAffinities[seasonIndex] * stats.weatherAffinities[weatherIndex] * stats.cycleAffinities[dayIndex] / stats.maxHP;
        // The affinities are hardcoded to the first element for now, but what we want to happen is for this script to receive a signal that will tell it what the time of day is, what the weather is, what the season is, and set the index of the affinities accordingly.
        cropHP = Mathf.Clamp(cropHP, 0f, stats.maxHP);
    }

    public void GetGrowth() // This uses cropHealth, cropMoisture and soilQuality to get cropGrowth, which represents how much a plant has grown overtime.
    {
        if (stats.maxHP <= 0)
        {
            return;
        }

        if (cropHP <= 0)
        {
            return;
        }

        float cropHealthBonus = (cropHP / stats.maxHP) * 20f;
        float cropMoistureBonus = BellCurve.GetFactor(cropMoisture, 15, 20) * 60f;
        float soilQualityBonus = (soilQuality / 100) * 20f;

        float rawCropGrowth = stats.standardGrowthSpeed * (100 + cropHealthBonus + cropMoistureBonus + soilQualityBonus) / 100;

        float maxCropGrowth = stats.standardGrowthSpeed * 2;

        cropGrowth += rawCropGrowth * stats.seasonalAffinities[seasonIndex] * stats.weatherAffinities[weatherIndex] * stats.cycleAffinities[dayIndex] / maxCropGrowth;
        cropGrowth = Mathf.Clamp(cropGrowth, 0f, 10f); // At the worst case scenario, we want the plant to stop growing instead of reversing in growth. To achieve this, we set its minimum value to 0.
    }

    public void GetHarvestQuality() // This uses cropHP and soilQuality to get the harvestQuality.
    {
        if (stats.maxHP <= 0)
        {
            return;
        }

        if (cropHP <= 0)
        {
            return;
        }

        float cropHealthBonus = (cropHP / stats.maxHP) * 50f;
        float soilQualityBonus = (soilQuality / 100) * 50f;

        float rawHarvestQuality = _harvestQualityMultiplier * (100 + cropHealthBonus + soilQualityBonus) / 100;

        if (soilQuality < 0) 
        {
            rawHarvestQuality *= -1; // If the soilQuality is outside its healthy threshold, set the rawHarvestQuality to a negative number as well. This will allow the harvestQuality to go up and down during gameplay.
        } 

        float maxHarvestQuality = stats.maxHP * 2;

        harvestQuality += (rawHarvestQuality * stats.seasonalAffinities[seasonIndex] * stats.weatherAffinities[weatherIndex] * stats.cycleAffinities[dayIndex] / maxHarvestQuality) * 100;
        harvestQuality = Mathf.Clamp(harvestQuality, 0f, 100f);
    }

    public int GetCropYield() // This uses the harvestQuality to generate a random number of harvested crops between a certain range.
    {
        if (stats.maxHP <= 0)
        {
            return 0;
        }

        if (cropHP <= 0)
        {
            return 0;
        }

        System.Random randomValue = new System.Random();

        if (harvestQuality >= 0 && harvestQuality <= 33)
        {
            return randomValue.Next((int)stats.badCropYield.x, (int)stats.badCropYield.y);
        }
        else if (harvestQuality >= 34 && harvestQuality <= 66)
        {
            return randomValue.Next((int)stats.averageCropYield.x, (int)stats.averageCropYield.y);
        }
        else if (harvestQuality >= 67 && harvestQuality <= 100)
        {
            return randomValue.Next((int)stats.goodCropYield.x, (int)stats.goodCropYield.y);
        }

        return 0;
    }
}
