using UnityEngine;

public class WinOrLoseManager : MonoBehaviour
{

    [Header("UI Items")]
    [SerializeField] public GameObject winScreen;
    [SerializeField] public GameObject loseScreen;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        winScreen.SetActive(false);
        loseScreen.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void onWin()
    {
        Debug.Log("WE STAY WINNING");
        winScreen.SetActive(true);
        FindFirstObjectByType<AudioManager>().Play("WinLevel");
    }
    public void onLose()
    {
        loseScreen.SetActive(true);
    }
}
