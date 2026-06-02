using UnityEngine;

#if UNITY_EDITOR
using Unity.Android.Gradle.Manifest;
using UnityEditor;
using UnityEditor.EditorTools;
#endif

[CreateAssetMenu(fileName = "NewSeed", menuName = "Sproutxel/SeedData")]
public class SeedData : ScriptableObject
{
    [Header("Basic Information")]
    public string cropName;
    public Sprite seedBagIcon;      // Show this in the hotbar

    [Header("Visual Growth States")]
    [Tooltip("Drag your 5 sprites here (0 = Seed, 4 = Mature Harvest Stage, 3 is wilting stage, 5 is death stage)")]
    public Sprite[] growthStages;

    [Header("Simulation Architecture Connection")]
    [Tooltip("Drag the specific PlantBase scriptable object asset created by your classmate here.")]
    public PlantBase plantStatsTemplate;

    [Header("Level Inventory Limits")]
    public int remainingSeedBags;

    [Header("Legacy/Fallback Settings")]
    [Tooltip("Keep this field to prevent other inventory scripts from breaking, though growth is now governed by stats math.")]
    public int ticksPerStage = 1;
}