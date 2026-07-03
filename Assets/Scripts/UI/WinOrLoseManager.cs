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
    [SerializeField] public GameObject BG;
    [SerializeField] public Animator WinLoseAnim;
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        winScreen.SetActive(false);
        loseScreen.SetActive(false);
       BG.SetActive(false);
        WinLoseAnim.SetFloat("Status", 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void onWin()
    {
        Debug.Log("WE STAY WINNING");
        winScreen.SetActive(true);
        AudioManager.instance.Play("WinLevel");
        WinLoseAnim.SetFloat("Status", 2);
        BG.SetActive(true);
        WinTween();

    }
    public void onLose()
    {
        loseScreen.SetActive(true);
        AudioManager.instance.Play("LoseLevel");
        LoseTween();
        BG.SetActive(true);
        WinLoseAnim.SetFloat("Status", 1);
    }

    public void WinTween()
    {
        BG.SetActive(true);
        WinScreenPos.DOAnchorPosY(MidPosY, tweenduration);

    }
    public void LoseTween()
    {
        BG.SetActive(true);
        LoseScreenPos.DOAnchorPosY(MidPosY, tweenduration);
    }
}
