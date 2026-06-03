using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName="Plant Base", menuName = "Items/Plant Base")]
public class PlantBase : ScriptableObject
{
    [Header("Name")]
    public string plantName;

    [Header("Health")]
    public int maxHP;

    [Header("Growth Speed")]
    public int standardGrowthSpeed;

    [Header("Harvest")] 
    public Vector2 badCropYield;
    public Vector2 averageCropYield;
    public Vector2 goodCropYield;
    // These store a range of values using the Vector2 variable. 
    //They represent the amount of crops that can be harvested depending on the plant's harvest quality. 
    //The x value represents the minimum amount, the y value represents the maximum.

    [Header("Resistances")]
    [Description("Dry and Wet | 0 is Dry | 1 is Wet")]
    public float[] seasonalAffinities; // 0 - Dry, 1 - Wet
    [Description("Clear, Heatwave and Typhoon | 0 is Clear | 1 is HeatWave | 2 is Typhoon")]
    public float[] weatherAffinities;  // 0 - Clear, 1 - Heat Wave, 2 - Typhoon
    [Description("Day and Night | 0 is Day | 1 is Night")]
    public float[] cycleAffinities;    // 0 - Day, 1 - Night
    [Description("Not Infested and Infested | 0 is Not | 1 is Is")]
    public float[] bugResistances;     // 0 - No bugs, 1 - Infested with bugs
}
