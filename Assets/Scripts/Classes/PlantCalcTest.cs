using UnityEngine;

public class PlantCalcTest : MonoBehaviour
{
    public BasePlant sample = new BasePlant();
    public PlantBase stats;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sample.stats = stats;
        sample.cropHP = stats.maxHP;
        sample.cropMoisture = 50f;
        sample.soilQuality = 100f;
        sample.soilMoisture = 50f;
        sample.soilSoftness = 50f;
    }

    // Update is called once per frame
    void Update()
    {
        do
        {
            sample.GetStatsOvertime();
            sample.GetSoilQuality();
            sample.GetHealth();
            sample.GetGrowth();
            sample.GetHarvestQuality();

            Debug.Log("Crop HP: " + sample.cropHP + "\n"
                      + "Crop Moisture: " + sample.cropMoisture + "\n"
                      + "Crop Growth: " + sample.cropGrowth + "\n"
                      + "Soil Quality: " + sample.soilQuality + "\n"
                      + "Soil Moisture: " + sample.soilMoisture + "\n"
                      + "Soil Softness: " + sample.soilSoftness + "\n"
                      + "Harvest Quality: " + sample.harvestQuality + "\n");
        } while (sample.cropGrowth < 100);

        if (sample.cropGrowth >= 100)
        {
            Debug.Log("Crop Yield: " + sample.GetCropYield());
        }
    }
}
