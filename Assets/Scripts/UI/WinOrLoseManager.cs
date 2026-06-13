using UnityEngine;
using DG.Tweening;
public class WinOrLoseManager : MonoBehaviour
{

    [Header("UI Items")]
    [SerializeField] public GameObject winScreen;
    [SerializeField] public GameObject loseScreen;
    [SerializeField] public RectTransform WinScreenPos,LoseScreenPos;
    [SerializeField] public float topPosY, MidPosY= 162.3098f;
    [SerializeField] public float tweenduration;
    


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
        WinTween();
    }
    public void onLose()
    {
        loseScreen.SetActive(true);
        FindFirstObjectByType<AudioManager>().Play("LoseLevel");
        LoseTween();
    }

    public void WinTween()
    {
        WinScreenPos.DOAnchorPosY(MidPosY, tweenduration);

    }
    public void LoseTween()
    {
        LoseScreenPos.DOAnchorPosY(MidPosY, tweenduration);
    }
}
