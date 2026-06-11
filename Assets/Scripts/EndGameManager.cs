using UnityEngine;

public class EndGameManager : MonoBehaviour
{
    [Header("Items")]
     public SeedManager[] seedsinScene;
    public int totalSeeds;
    public WinOrLoseManager winlosr;
    void Start()
    {
        seedsinScene = Object.FindObjectsByType<SeedManager>(FindObjectsSortMode.None);

        foreach (SeedManager ses in seedsinScene)
        {
            totalSeeds += ses.available;
        }
    }
    public void LoseSeed()
    {
        totalSeeds--;
    }
    // Update is called once per frame
    void Update()
    {
        if (totalSeeds <= 0) { winlosr.onLose(); }

    }
}
