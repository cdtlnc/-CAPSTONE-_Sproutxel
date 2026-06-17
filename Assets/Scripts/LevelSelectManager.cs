using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    [SerializeField] private Button[] levelButtons; // Buttons array for each level scene
    [SerializeField] private string transitionName = "CrossFade";

    void Start()
    {
        // Loops over buttons array and unlocks all of them
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1;        // level numbers start from one
            string sceneName = "Level_" + levelNumber;

            // CHANGE: Always set to true so all levels are accessible
            levelButtons[i].interactable = true;

            levelButtons[i].onClick.RemoveAllListeners();   // removes any existing click listeners to avoid duplicates
            levelButtons[i].onClick.AddListener(() =>   // capture sceneName with a local variable to prevent closure issues (button errors)
            {
                LoadLevel(sceneName);
            });
        }
    }

    void LoadLevel(string sceneName)
    {
        AudioManager.instance.Play("TapSound1");
        AudioManager.instance.Stop("MainMenu");
        AudioManager.instance.Stop("LevelSelectMenu");
        AudioManager.instance.Play("SproutxelBGMusic");

        LevelManager.Instance.LoadScene(sceneName, transitionName);
    }
}
