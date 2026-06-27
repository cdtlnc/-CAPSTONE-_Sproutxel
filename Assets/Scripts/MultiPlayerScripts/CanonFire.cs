using UnityEngine;

public class CanonFire : MonoBehaviour
{
    [Header("Canon Attributes")]
    [SerializeField] public GameObject canonball;
    [SerializeField] public Transform canonballSpawnPoint;
    [SerializeField] public GameObject vfx;
    [SerializeField] public Transform vfxspawnpoint;
    [SerializeField] public GoalManager_Multiplayer AssignedGoalManager;
    [SerializeField] public Animator canonAnim;
    [SerializeField] public FarmerHealth enemy;
    [SerializeField] public GrowthManager_Multiplayer[] selectedPlots;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddLoad(int yield)
    {
        Debug.Log("[STEP 3] FIRING SEED. Yield: "+ yield);
        canonAnim.SetTrigger("_IsFiring");
        AudioManager.instance.Play("CanonSFX1");
        enemy.DepleteHealth(yield);
    }
    public void CommitAnimations(int yield)
    {
        
        var bug = Instantiate(canonball, canonballSpawnPoint.position, canonballSpawnPoint.rotation);
        bug.GetComponent<Rigidbody>().linearVelocity = canonballSpawnPoint.forward * 10000f;
        Instantiate(vfx, vfxspawnpoint.position, Quaternion.identity);
    }

    public void GiveBugs()//Pesticide
    {
        canonAnim.SetTrigger("_IsFiring");
        foreach (GrowthManager_Multiplayer gm in selectedPlots) 
        {
            gm.CommitAction("GETBUGGED");
            CommitAnimations(0);
        }
    }
    public void GetWaterLogged()//Soil Addler
    {
        canonAnim.SetTrigger("_IsFiring");
        foreach (GrowthManager_Multiplayer gm in selectedPlots)
        {
            gm.CommitAction("GetWaterLogged");
            CommitAnimations(0);
        }
    }
    public void SOILEDIT()//SoilTiller
    {
        canonAnim.SetTrigger("_IsFiring");
        foreach (GrowthManager_Multiplayer gm in selectedPlots)
        {
            CommitAnimations(0);
            gm.CommitAction("UnTillable");
        }
    }
    public void RemoveLePlants()//Shovel
    {
        canonAnim.SetTrigger("_IsFiring");
        foreach (GrowthManager_Multiplayer gm in selectedPlots)
        {
            gm.CommitAction("RemovePlants");
            CommitAnimations(0);
        }
    }
    public void GetOld()//
    {
        canonAnim.SetTrigger("_IsFiring");
        foreach (GrowthManager_Multiplayer gm in selectedPlots)
        {
            gm.CommitAction("FERTILIZING");
            CommitAnimations(0);
        }
    }

}
