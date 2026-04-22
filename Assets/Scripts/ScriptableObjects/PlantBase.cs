using UnityEngine;

[CreateAssetMenu(fileName="Plant Base", menuName = "Items/Plant Base")]
public class PlantBase : ScriptableObject
{
    [Header("Name")]
    public string plantName;

    [Header("Health")]
    public int maxHP;
    public int cropHP;

    [Header("Maturation Window")]
    public Vector2 maturationWindow;
    public int decayRate;

    [Header("Growth Speed")]
    public int standardGrowthSpeed;
    public float cropGrowthSpeed;
    public float cropGrowth;

    [Header("Harvest")]
    public Vector2 badCropYield;
    public Vector2 averageCropYield;
    public Vector2 goodCropYield;
    public float harvestQuality;

    [Header("Resistances")]
    public float[] seasonalAffinities; // 0 - Dry, 1 - Wet
    public float[] weatherAffinities;  // 0 - Heat Wave, 1 - Typhoon
    public float[] cycleAffinities;    // 0 - Day, 1 - Night
}
