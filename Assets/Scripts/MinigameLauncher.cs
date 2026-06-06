using UnityEngine.SceneManagement;

public static class MinigameLauncher
{
    // Holds the live reference to the plot that triggered the minigame
    public static GrowthManager activePlot;

    /// <summary>
    /// Launches a minigame scene additively, keeping the main farm scene alive in the background.
    /// </summary>
    public static void LaunchMinigame(string sceneName, GrowthManager plot)
    {
        activePlot = plot;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }
}