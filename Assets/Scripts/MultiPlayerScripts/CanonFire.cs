using UnityEngine;

public class CanonFire : MonoBehaviour
{
    [Header("Canon Attributes")]
    [SerializeField] public GameObject canonball;
    [SerializeField] public GameObject vfx;
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
        Instantiate(canonball, Vector3.zero, Quaternion.identity);
        Instantiate(vfx, Vector3.zero, Quaternion.identity);
        enemy.DepleteHealth(yield);
    }
}
