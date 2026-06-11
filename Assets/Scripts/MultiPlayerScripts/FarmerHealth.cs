using UnityEngine;
using UnityEngine.UI;

public class FarmerHealth : MonoBehaviour
{
    [Header("Farmer ")]
    [SerializeField] public int Health;
    [SerializeField] public Slider HealthSlider;
    [SerializeField] public GoalManager_Multiplayer GoalManagerMulti;
    [SerializeField] public int damagedealt;
    [SerializeField] public string Name;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HealthSlider.maxValue= Health;
        HealthSlider.value = Health;
        HealthSlider.minValue= 0;
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DepleteHealth(int damage)
    {
       
        Health -= damage;
        Debug.Log("[STEP 4] DEPLETING HEALTH. Healht: " + Health + " Damage: " + damage);
        Checkhealth();
        UpdateUI();
        
    }

    public void GetInfested()
    {
        
    }

    public void GetWaterlogged()
    {

    }

    public void Checkhealth()
    {
        Debug.Log("[STEP 5] CHECKING HEALTH");
        if (Health <= 0)
        {
            GoalManagerMulti.LoseGame(name);
        }
    }

    public void UpdateUI()
    {
        HealthSlider.value = Health;
    }

}
