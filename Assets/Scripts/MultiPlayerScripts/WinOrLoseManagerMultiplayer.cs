using TMPro;
using UnityEngine;

public class WinOrLoseManager_Multiplayer : MonoBehaviour
{

    [Header("UI Items")]
    [SerializeField] public GameObject p1Screen;
    [SerializeField] public GameObject p2Screen;
    [SerializeField] public string[] WinLoseText = { "Winner", "Loser" };
    [SerializeField] public TextMeshProUGUI player1;
    [SerializeField] public TextMeshProUGUI player2;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        p1Screen.SetActive(false);
        p2Screen.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void onWin()
    {
        Debug.Log("WE STAY WINNING");
        p1Screen.SetActive(true);
        p2Screen.SetActive(true);
        FindFirstObjectByType<AudioManager>().Play("WinLevel");
    }
    public void onLose(string loser)
    {
        p1Screen.SetActive(true);
        p2Screen.SetActive(true);
        if (loser == "Farmer1")
        {
            player1.text = WinLoseText[1];
            player2.text = WinLoseText[0];
        }
        else
        {
            player2.text = WinLoseText[1];
            player1.text = WinLoseText[0];
        }


        FindFirstObjectByType<AudioManager>().Play("WinLevel");
    }
}
