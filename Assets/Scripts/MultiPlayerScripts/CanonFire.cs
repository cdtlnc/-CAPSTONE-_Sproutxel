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

}
