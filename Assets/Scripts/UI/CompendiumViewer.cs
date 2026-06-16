using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompendiumViewer : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private GameObject CompendiumPanel;
    [SerializeField] private GameObject MainMenuPanel;
   
    [SerializeField] private Sprite[] plantIcons;
    [SerializeField] private Image plantSpriter;
    [SerializeField] private Image SeasonalIcons;
    [SerializeField] private Image CycleIcons;
    [SerializeField] private TextMeshProUGUI name;


    public void Open()
    {
        MainMenuPanel.SetActive(false);
        CompendiumPanel.SetActive(true);
    }
    public void Close()
    {
        CompendiumPanel.SetActive(false);
        MainMenuPanel.SetActive(true);
    }
    public void UpdateUI(SeedData plantinfo)
    {
        AudioManager.instance.Play("CompendiumTap");
        name.text= plantinfo.name;
        plantSpriter.sprite = plantinfo.growthStages[3];
        if (plantinfo.plantStatsTemplate.seasonalAffinities[0] < 1)//
        {
            SeasonalIcons.sprite = plantIcons[2];
        }
        else
        {
            SeasonalIcons.sprite = plantIcons[3];
        }

        if (plantinfo.plantStatsTemplate.cycleAffinities[0] < 1)//Daily and Nightly
        {
            CycleIcons.sprite = plantIcons[0];
        }
        else if(plantinfo.plantStatsTemplate.cycleAffinities[0] == 1)
        {
            CycleIcons.sprite = plantIcons[4];
        }
        else
        {
            CycleIcons.sprite = plantIcons[1];
        }

    }

  
}
