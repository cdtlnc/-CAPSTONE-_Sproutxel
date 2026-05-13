using UnityEngine;

[CreateAssetMenu(fileName = "NewSeed", menuName = "Sproutxel/SeedData")]
public class SeedData : ScriptableObject
{
    public string cropName;
    public Sprite seedBagIcon;      // Show this in the hotbar
    public Sprite[] growthStages;   // Drag your 4 sprites here (Seed to Harvest)
    public int ticksPerStage = 1;   // How many 20s ticks needed to grow
}   