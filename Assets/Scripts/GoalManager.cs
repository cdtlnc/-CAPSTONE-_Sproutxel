using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class CropGoalProgress
{
    public string cropName;
    public int target;
    public int harvested = 0;
}

public class GoalManager : MonoBehaviour
{
    [Header("Level Data")]
    [SerializeField] private LevelGoalData levelGoalData;

    [Header("UI")]
    [SerializeField] public TMP_Text goalText;

    private List<CropGoalProgress> _progress = new List<CropGoalProgress>();

    private void Start()
    {
        BuildProgressFromLevelData();
        UpdateUI();
    }

    private void BuildProgressFromLevelData()
    {
        _progress.Clear();

        if (levelGoalData == null)
        {
            Debug.LogWarning("[GoalManager] No LevelGoalData assigned! Assign one in the Inspector.");
            return;
        }

        foreach (var def in levelGoalData.cropGoals)
        {
            _progress.Add(new CropGoalProgress
            {
                cropName = def.cropName,
                target = def.target,
                harvested = 0
            });
        }
    }

    public void AddCrop(string cropName, int yield)
    {
        CropGoalProgress entry = _progress.Find(g => g.cropName == cropName);

        if (entry == null)
        {
            Debug.Log($"[GoalManager] '{cropName}' is not a required crop for this level, ignoring.");
            return;
        }

        AudioManager.instance.Play("Harvest");

        entry.harvested = Mathf.Min(entry.harvested + yield, entry.target);

        Debug.Log($"[GoalManager] Harvested {yield} {cropName} ({entry.harvested}/{entry.target})");

        UpdateUI();
        checkObjectives();
    }

    private void UpdateUI()
    {
        if (goalText == null) return;

        var lines = new System.Text.StringBuilder();
        foreach (var entry in _progress)
        {
            bool complete = entry.harvested >= entry.target;
            string line = $"{entry.cropName}: {entry.harvested}/{entry.target}";

            if (complete)
                line = $"<s>{line}</s>";

            lines.AppendLine(line);
        }

        goalText.text = lines.ToString().TrimEnd();
    }

    public void checkObjectives()
    {
        foreach (var entry in _progress)
        {
            if (entry.harvested < entry.target)
                return;
        }

        Debug.Log("[GoalManager] All crop quotas met — sending to WinOrLoseManager.");
        WinOrLoseManager w = FindAnyObjectByType<WinOrLoseManager>();
        if (w != null) w.onWin();
    }
}