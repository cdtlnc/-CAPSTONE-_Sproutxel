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
    public int bugIndex { private get; set; }     // This is either 0 or 1. 0 - No infestation, 1 - Infested

    // These values manage how much these stats change per tick by default. These are NOT external modifiers. Be careful when tinkering with them.
    private float _cropMoistureMultiplier   = 2f;   // 2.0f by default
    private float _soilMoistureMultiplier   = 2f;   // 2.0f by default
    private float _soilSoftnessMultiplier   = 2f;   // 2.0f by default
    private float _soilQualityMultiplier    = 2.3f; // 2.3f by default
    private float _cropHealthMultiplier     = 0.6f; // 0.6f by default
    private float _harvestQualityMultiplier = 1.9f; // 1.9f by default

    public float seasonIndexPlant; // This is either 0 or 1. 0 - Dry, 1 - Wet
    public float dayIndexPlant;   // This is either 0 or 1. 0 - Day, 1 - Night
    public float weatherIndexPlant; // This is either 0, 1 or 2. 0 - No weather event, 1 - Heat Wave, 2 - Typhoon
    public float bugIndexPlant;

    public void GetStatsOvertime() // This calculates the depreciation of the cropMoisture, soilMoisture and soilSoftness stats
    {
        float cropMoistureMultiplier = 1; 
        float soilMoistureMultiplier = 1; 
        float soilSoftnessMultiplier = 1;
        weatherIndexPlant = stats.weatherAffinities[weatherIndex];
        seasonIndexPlant = stats.seasonalAffinities[seasonIndex];
        dayIndexPlant = stats.cycleAffinities[dayIndex];
        // The following if-else statements affect the changes in the following stats depending on the season and the weather.
        // Setting it to a negative or a positive number makes the stat go down and up respectively.
        // Setting it below 1 makes the change slower. I recommend keeping it between 0 and 1. Think of it as 0% and 100%.
        // Setting it above 1 makes it faster. I recommend keeping it between 1 and 2. Think of it as 100% and 200%.
        // Tinker with the values to achieve the rate of change that you want.

        //Dry Or Wet
        if (seasonIndex == 0)//Dry 
        {
            cropMoistureMultiplier += -1.8f;
            soilMoistureMultiplier += -1.8f;
            soilSoftnessMultiplier += 1.2f;
            
        }
        else if (seasonIndex == 1)//Wet
        {
            cropMoistureMultiplier += 1.8f;
            soilMoistureMultiplier += 1.8f;
            soilSoftnessMultiplier += -1.2f;
          
        }

        //Heat Haze or Typhoon
        if (weatherIndex == 1)//Heat Haze
        {
            cropMoistureMultiplier += -1.8f;
            soilMoistureMultiplier += -1.8f;
            soilSoftnessMultiplier += 1.2f;
           

        }

        else if (weatherIndex == 2)//Typhoon
        {
            cropMoistureMultiplier += 1.8f;
            soilMoistureMultiplier += 1.8f;
            soilSoftnessMultiplier += -1.2f;
         
        }


        //Day or Night
        if (dayIndex == 0)//Day
        {
            cropMoistureMultiplier += -1.8f;
            soilMoistureMultiplier += -1.8f;
            soilSoftnessMultiplier += -1.2f;
         

        }

        else if (dayIndex == 1)//Night
        {
            cropMoistureMultiplier += 1.8f;
            soilMoistureMultiplier += 1.8f;
            soilSoftnessMultiplier += 1.2f;
     
        }


        cropMoisture += _cropMoistureMultiplier * cropMoistureMultiplier * seasonIndexPlant * weatherIndexPlant * dayIndexPlant;
        soilMoisture += _soilMoistureMultiplier * soilMoistureMultiplier * seasonIndexPlant * weatherIndexPlant * dayIndexPlant;
        soilSoftness += _soilSoftnessMultiplier * soilSoftnessMultiplier * seasonIndexPlant * weatherIndexPlant * dayIndexPlant;

        cropMoisture = Mathf.Clamp(cropMoisture, -100f, 100f);
        soilMoisture = Mathf.Clamp(soilMoisture, -100f, 100f);
        soilSoftness = Mathf.Clamp(soilSoftness, -100f, 100f);
    }

    public void GetSoilQuality()
    {
        float soilMoistureBonus = BellCurve.GetFactor(soilMoisture, 15, 20) * 50f;
        float soilSoftnessBonus = BellCurve.GetFactor(soilSoftness, 15, 20) * 50f;

        float rawSoilQualityChange = _soilQualityMultiplier * (soilMoistureBonus + soilSoftnessBonus) / 50;

        // Change element 0 in unity inspector to 0 (no penalty)
        // Change element 1 in unity inspector to 15 or 20 (flat penalty deduction)
        float bugPenalty = stats.bugResistances[bugIndex];

        // Apply the clean change, then subtract the infestation penalty
        soilQuality += (rawSoilQualityChange) - bugPenalty;
        soilQuality = Mathf.Clamp(soilQuality, -100f, 100f);
    }

    public void GetHealth() // This uses soilQuality, soilMoisture, soilSoftness and cropMoisture to get cropHP
    {
        if (stats.maxHP <= 0)
        {
            return;
        }
        /*
        if (cropHP <= 0)
        {
            return;
        }
            */
        float soilQualityBonus  = (soilQuality / 100) * 25;
        float soilMoistureBonus = BellCurve.GetFactor(soilMoisture, 15, 20) * 25f;
        float soilSoftnessBonus = BellCurve.GetFactor(soilSoftness, 15, 20) * 25f;
        float cropMoistureBonus = BellCurve.GetFactor(cropMoisture, 15, 20) * 25f;

        float rawHealthChange = _cropHealthMultiplier * (soilQualityBonus + soilMoistureBonus + soilSoftnessBonus + cropMoistureBonus) / 100;

        cropHP += rawHealthChange * stats.seasonalAffinities[seasonIndex] * stats.weatherAffinities[weatherIndex] * stats.cycleAffinities[dayIndex] * stats.bugResistances[bugIndex] / stats.maxHP;
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
        cropGrowth = Mathf.Clamp(cropGrowth, 0f, 15f); // At the worst case scenario, we want the plant to stop growing instead of reversing in growth. To achieve this, we set its minimum value to 0.
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
        /*
        if (cropHP <= 0)
        {
            return 0;
        }
        */

        System.Random randomValue = new System.Random();


        // FIXED: Changed thresholds to direct inequalities (< and >=) so float fractions (like 66.5) don't slip through
        if (harvestQuality < 34f)
        {
            // FIXED: Added + 1 to upper limits because System.Random.Next is exclusive of the max bound
            return randomValue.Next((int)stats.badCropYield.x, (int)stats.badCropYield.y + 1);
        }
        else if (harvestQuality >= 34f && harvestQuality < 67f)
        {
            return randomValue.Next((int)stats.averageCropYield.x, (int)stats.averageCropYield.y + 1);
        }
        else // This handles everything from 67 to 100+ safely
        {
            return randomValue.Next((int)stats.goodCropYield.x, (int)stats.goodCropYield.y + 1);
        }



        return 1;
    }

    public int GetMaxYield()
    {
        System.Random randomValue = new System.Random();
        return randomValue.Next((int)stats.goodCropYield.x, (int)stats.goodCropYield.y);

        return 0;
    }

    public int GetBadYield()
    {
        System.Random randomValue = new System.Random();
        return randomValue.Next((int)stats.badCropYield.x, (int)stats.badCropYield.y);

        return 0;
    }

    public int GetMidYield()
    {
        System.Random randomValue = new System.Random();
        return randomValue.Next((int)stats.averageCropYield.x, (int)stats.averageCropYield.y);

        return 0;
    }
}
