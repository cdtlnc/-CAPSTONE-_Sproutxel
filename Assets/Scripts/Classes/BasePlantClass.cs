using UnityEngine;

public class BasePlant 
{
    public PlantBase stats;
    public Sprite[] growthStages; // Put your 4 sprites here (0 = Seed, 3 = Mature)

    public float cropHP;         // 0 to crop type's max HP
    public float cropMoisture;   // -100 to 100, healthy range is -20 to 50
    public float cropGrowth = 0;     // 0 to 100

    public float soilQuality;  // -100 to 100
    public float soilMoisture; // -100 to 100, healthy range is -20 to 50
    public float soilSoftness; // -100 to 100, healthy range is -20 to 50

    public float harvestQuality; // 0 to 100

    public void GetStatsOvertime() // This calculates the depreciation of the cropMoisture, soilMoisture and soilSoftness stats
    {
        cropMoisture -= 2 * (100 + cropMoisture) / 100;
        soilMoisture -= 2 * (100 + soilMoisture) / 100;
        soilSoftness -= 2 * (100 + soilSoftness) / 100;

        cropMoisture = Mathf.Clamp(cropMoisture, -100f, 100f);
        soilMoisture = Mathf.Clamp(soilMoisture, -100f, 100f);
        soilSoftness = Mathf.Clamp(soilSoftness, -100f, 100f);
    }

    public void GetSoilQuality() // This uses soilMoisture and soilSoftness to calculate soilQuality
    {
        float soilMoistureBonus = (soilMoisture / 100) * 50;
        float soilSoftnessBonus = (soilSoftness / 100) * 50;

        float rawSoilQualityChange = 10 * (100 + soilMoistureBonus + soilSoftnessBonus) / 100;

        if (soilSoftness < -20 || soilMoisture < -20)
        {
            rawSoilQualityChange *= -1; // If any of these states are below the healthy threshold, set the rawHealthChange to a negative number as well. This will allow the cropHP to go up and down during gameplay.
        }
        else if (soilSoftness > 50 || soilMoisture > 50)
        {
            rawSoilQualityChange *= -1; // Same deal, just if they're above the healthy threshold. Ideally, I want to change these formulas so their effects resemble a bell curve, but for now, this will do.
        }

        soilQuality += (rawSoilQualityChange / 100) * 100;
        soilQuality = Mathf.Clamp(soilQuality, -100f, 100f);
    }

    public void GetHealth() // This uses soilQuality, soilMoisture, soilSoftness and cropMoisture to get cropHP
    {
        float soilQualityBonus  = (soilQuality / 100) * 25;
        float soilMoistureBonus = (soilMoisture / 100) * 25;
        float soilSoftnessBonus = (soilSoftness / 100) * 25;
        float cropMoistureBonus = (cropMoisture / 100) * 25;

        float rawHealthChange = stats.maxHP * (100 + soilQualityBonus + soilMoistureBonus + soilSoftnessBonus + cropMoistureBonus) / 100;

        if (soilQuality < 0 || soilSoftness < -20 || soilMoisture < -20 || cropMoisture < -20)
        {
            rawHealthChange *= -1;
        }
        else if (soilSoftness > 50 || soilMoisture > 50 || cropMoisture > 50)
        {
            rawHealthChange *= -1;
        }

        cropHP += rawHealthChange * stats.cycleAffinities[0] * stats.weatherAffinities[0] * stats.cycleAffinities[0] / stats.maxHP;
        // The affinities are hardcoded to the first element for now, but what we want to happen is for this script to receive a signal that will tell it what the time of day is, what the weather is, what the season is, and set the index of the affinities accordingly.
        cropHP = Mathf.Clamp(cropHP, 0f, stats.maxHP);
    }

    public void GetGrowth() // This uses cropHealth, cropMoisture and soilQuality to get cropGrowth, which represents how much a plant has grown overtime.
    {
        float cropHealthBonus = (cropHP / stats.maxHP) * 40;
        float cropMoistureBonus = (cropMoisture / 100) * 30;
        float soilQualityBonus = (soilQuality / 100) * 30;

        float rawCropGrowth = stats.standardGrowthSpeed * (100 + cropHealthBonus + cropMoistureBonus + soilQualityBonus) / 100;

        if (cropMoisture < -20 || soilQuality < 0)
        {
            rawCropGrowth *= -1;
        }

        else if (cropMoisture < 50)
        {
            rawCropGrowth *= -1;
        }

        float maxCropGrowth = stats.standardGrowthSpeed * 2;

        cropGrowth += rawCropGrowth * stats.cycleAffinities[0] * stats.weatherAffinities[0] * stats.cycleAffinities[0] / maxCropGrowth;
        cropGrowth = Mathf.Clamp(cropGrowth, 0f, 100f); // At the worst case scenario, we want the plant to stop growing instead of reversing in growth. To achieve this, we set its minimum value to 0.
    }

    public void GetHarvestQuality() // This uses cropHP and soilQuality to get the harvestQuality.
    {
        float rawHarvestQuality = cropHP * (100 + soilQuality) / 100;

        if (soilQuality < 0) 
        {
            rawHarvestQuality *= -1; // If the soilQuality is outside its healthy threshold, set the rawHarvestQuality to a negative number as well. This will allow the harvestQuality to go up and down during gameplay.
        } 

        float maxHarvestQuality = stats.maxHP * 2;

        harvestQuality += (rawHarvestQuality * stats.cycleAffinities[0] * stats.weatherAffinities[0] * stats.cycleAffinities[0] / maxHarvestQuality) * 100;
        harvestQuality = Mathf.Clamp(harvestQuality, 0f, 100f);
    }

    public int GetCropYield() // This uses the harvestQuality to generate a random number of harvested crops between a certain range.
    {
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
