using UnityEngine;
using Photon.Pun;

public class CanonFire : MonoBehaviourPun
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

    // This gets called by your items/seeds locally
    public void AddLoad(int yield)
    {
        Debug.Log("[MULTIPLAYER] FIRING SEED. Yield: " + yield);

        // Sync the firing animations, sound, and damage across the network
        photonView.RPC("RPC_ExecuteFire", RpcTarget.All, yield);
    }

    [PunRPC]
    private void RPC_ExecuteFire(int yield)
    {
        if (canonAnim != null) canonAnim.SetTrigger("_IsFiring");

        if (AudioManager.instance != null) AudioManager.instance.Play("CanonSFX1");

        // Deal damage directly to the enemy health script (which is also networked now!)
        if (enemy != null) enemy.DepleteHealth(yield);

        // Spawn the physics cannonball locally on every client
        CommitAnimations(yield);
    }

    public void CommitAnimations(int yield)
    {
        // Spawning visual objects locally per client keeps performance smooth
        var bug = Instantiate(canonball, canonballSpawnPoint.position, canonballSpawnPoint.rotation);

        Rigidbody rb = bug.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = canonballSpawnPoint.forward * 10000f;
        }

        Instantiate(vfx, vfxspawnpoint.position, Quaternion.identity);
    }

    // --- NETWORKED EXTRA ACTIONS ---
    // These methods now broadcast via RPC so status effects hit all plots on both screens

    public void GiveBugs() // Pesticide
    {
        photonView.RPC("RPC_NetworkAction", RpcTarget.All, "GETBUGGED");
    }

    public void GetWaterLogged() // Soil Addler
    {
        photonView.RPC("RPC_NetworkAction", RpcTarget.All, "GetWaterLogged");
    }

    public void SOILEDIT() // SoilTiller
    {
        photonView.RPC("RPC_NetworkAction", RpcTarget.All, "UnTillable");
    }

    public void RemoveLePlants() // Shovel
    {
        photonView.RPC("RPC_NetworkAction", RpcTarget.All, "RemovePlants");
    }

    public void GetOld() // Fertilizer
    {
        photonView.RPC("RPC_NetworkAction", RpcTarget.All, "FERTILIZING");
    }

    [PunRPC]
    private void RPC_NetworkAction(string actionType)
    {
        if (canonAnim != null) canonAnim.SetTrigger("_IsFiring");

        foreach (GrowthManager_Multiplayer gm in selectedPlots)
        {
            if (gm != null)
            {
                gm.CommitAction(actionType);
                CommitAnimations(0);
            }
        }
    }
}