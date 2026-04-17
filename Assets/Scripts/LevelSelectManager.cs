using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    [SerializeField] private Button[] levelButtons; // Buttons array for each level scene
    [SerializeField] private string transitionName = "CrossFade";

    void Start()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1); // Stores which level is unlocked

        for (int i = 0; i < levelButtons.Length; i++)               // Loops over buttons array
        {
            int levelNumber = i + 1;        // level numbers start from one
            string sceneName = "Level_" + levelNumber;

            levelButtons[i].interactable = levelNumber <= unlockedLevel;    // enable or disable button depending on whether the level is unlocked

            levelButtons[i].onClick.RemoveAllListeners();   // removes any existing click listeners to avoid duplicates
            levelButtons[i].onClick.AddListener(() =>   // capture sceneName with a local variable to prevent closure issues (button errors)
            {
                LoadLevel(sceneName);
            });
        }
    }

    void LoadLevel(string sceneName)
    {
        LevelManager.Instance.LoadScene(sceneName, transitionName);
    }
}
