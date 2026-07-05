using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutoriallLoader : MonoBehaviour
{
    public GameObject tutorialPanel;
    public Image TutorialRenderer;
    public Sprite[] Tutorial1, Tutorial2;
    public TextMeshProUGUI tutorialLength;
    public int Selected_Tutorial;
    public int Selected_Tutorial_Length;
    public int ImageInt;
    void Start()
    {
        tutorialPanel.SetActive(false);
        ImageInt = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Tut1()
    {
        Debug.Log("EnteredTutorial.1");
        Selected_Tutorial = 1;
        Selected_Tutorial_Length = Tutorial1.Length;
        OpenPanel();
    }
    public void Tut2()
    {
        Selected_Tutorial = 2;
        Selected_Tutorial_Length = Tutorial2.Length;
        OpenPanel();
    }
  
    public void OpenPanel()
    {
        tutorialPanel.SetActive(true);
       UpdateUI();
    }
    public void UpdateUI()
    {
       
        switch (Selected_Tutorial)
        {
            case 1:
                TutorialRenderer.sprite = Tutorial1[ImageInt];
                break;
            case 2:
                TutorialRenderer.sprite = Tutorial2[ImageInt];
                break;
            
        }

        tutorialLength.text = (ImageInt+1) +" / "+Selected_Tutorial_Length;
    }
    public void Back()
    {
        Debug.Log("Reached Back");
        if (ImageInt == 0) return;
        ImageInt--;
        UpdateUI();
        AudioManager.instance.Play("CompendiumTap");
    }
    public void Forward()
    {
        Debug.Log("Reached Forward");
        if (ImageInt == Selected_Tutorial_Length ) return;
        Debug.Log("Reached Past Forward");
        ImageInt++;
        AudioManager.instance.Play("CompendiumTap");
        UpdateUI();
    }
    public void Exit()
    {
        tutorialPanel.SetActive(false);
        Selected_Tutorial = 0;
        ImageInt = 0;
        Selected_Tutorial_Length = 0;
    }
}
