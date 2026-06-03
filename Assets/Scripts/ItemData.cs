using UnityEngine;

#if UNITY_EDITOR
using Unity.Android.Gradle.Manifest;
using UnityEditor;
using UnityEditor.EditorTools;
#endif

[CreateAssetMenu(fileName = "NewSeed", menuName = "Sproutxel/SeedData")]
public class ItemData : ScriptableObject
{
    

    [Header("Level Inventory Limits")]
    public int remainingSeedBags;

    [Header("Legacy/Fallback Settings")]
    [Tooltip("Keep this field to prevent other inventory scripts from breaking, though growth is now governed by stats math.")]
    public int ticksPerStage = 1;
}