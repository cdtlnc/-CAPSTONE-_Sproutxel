using UnityEngine;

public class PlantInfo : MonoBehaviour
{

    
    public SeedData selectedplant;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public void sendInfo()
    {
        CompendiumViewer cuh = FindFirstObjectByType<CompendiumViewer>();
        cuh.UpdateUI(selectedplant);
    }

}
