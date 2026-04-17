using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuUI : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private GameObject optionsPanel;

    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        if (OptionsManager.Instance != null)        // Initialize sliders from OptionsManager
        {
            masterSlider.value = OptionsManager.Instance.MasterVolume;
            musicSlider.value = OptionsManager.Instance.MusicVolume;
            sfxSlider.value = OptionsManager.Instance.SFXVolume;
        }

        masterSlider.onValueChanged.AddListener((v) =>      // Add listeners to update OptionsManager when slider changes
        {
            if (OptionsManager.Instance != null)
                OptionsManager.Instance.MasterVolume = v;
        });

        musicSlider.onValueChanged.AddListener((v) =>
        {
            if (OptionsManager.Instance != null)
                OptionsManager.Instance.MusicVolume = v;
        });

        sfxSlider.onValueChanged.AddListener((v) =>
        {
            if (OptionsManager.Instance != null)
                OptionsManager.Instance.SFXVolume = v;
        });
    }

    public void Open()
    {
        optionsPanel.SetActive(true);
    }

    public void Close()
    {
        if (OptionsManager.Instance != null)        // Save settings when closing
            OptionsManager.Instance.SaveSettings();

        optionsPanel.SetActive(false);
    }
}
