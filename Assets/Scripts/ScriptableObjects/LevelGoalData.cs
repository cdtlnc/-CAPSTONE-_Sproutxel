using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CropGoalDefinition
{
    public string cropName;
    public int target;
}

[CreateAssetMenu(fileName = "Level Goal Data", menuName = "LGD/Level Goal Data")]
public class LevelGoalData : ScriptableObject
{
    public List<CropGoalDefinition> cropGoals = new List<CropGoalDefinition>();
}