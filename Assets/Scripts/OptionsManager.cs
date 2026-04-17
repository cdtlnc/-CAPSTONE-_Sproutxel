using UnityEngine;

public class OptionsManager : MonoBehaviour
{
    public static OptionsManager Instance;

    [Header("SliderVolumes")]           // slider volume levels, set to 1 (100%)
    public float MasterVolume = 1f;
    public float MusicVolume = 1f;
    public float SFXVolume = 1f;

    void Awake()
    {
        if (Instance == null)       // singleton
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveSettings()      // save slider volume levels
    {
        PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
        PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
        PlayerPrefs.SetFloat("SFXVolume", SFXVolume);
        PlayerPrefs.Save();
    }
}
