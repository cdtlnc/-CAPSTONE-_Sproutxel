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

    [Header("Ghost cannon")]
    [SerializeField] private GhostCanonView ghostCanon;

    // HARVEST DAMAGE

    public void AddLoad(int yield)
    {
        Debug.Log("AddLoad called on: " + gameObject.name);
        Debug.Log($"[CanonFire] Firing. Yield: {yield}");

        if (canonAnim != null)
            canonAnim.SetTrigger("_IsFiring");

        AudioManager.instance.Play("CanonSFX1");

        if (enemy != null && enemy.photonView != null)
            enemy.photonView.RPC("RPC_TakeDamage", RpcTarget.Others, yield);

        if (ghostCanon != null && ghostCanon.photonView != null)
            ghostCanon.photonView.RPC("RPC_PlayGhostCanon", RpcTarget.Others);
    }

    // LOCAL ANIMATIONS

    public void CommitAnimations(int yield)
    {
        if (canonball != null && canonballSpawnPoint != null)
        {
            var ball = Instantiate(
                canonball,
                canonballSpawnPoint.position,
                canonballSpawnPoint.rotation);

            ball.GetComponent<Rigidbody>().linearVelocity =
                canonballSpawnPoint.forward * 10000f;
        }

        if (vfx != null && vfxspawnpoint != null)
        {
            Instantiate(
                vfx,
                vfxspawnpoint.position,
                Quaternion.identity);
        }
    }

    // SABOTAGE RPC

    [PunRPC]
    public void RPC_ApplySabotage(string action)
    {
        Debug.Log("[CanonFire] Received sabotage: " + action);

        if (canonAnim != null)
            canonAnim.SetTrigger("_IsFiring");

        AudioManager.instance.Play("CanonSFX1");
        CommitAnimations(0);

        if (selectedPlots == null || selectedPlots.Length == 0)
            return;

        // Build a list of only planted plots
        System.Collections.Generic.List<GrowthManager_Multiplayer> plantedPlots =
            new System.Collections.Generic.List<GrowthManager_Multiplayer>();

        foreach (GrowthManager_Multiplayer plot in selectedPlots)
        {
            if (plot != null && plot.isPlanted)
                plantedPlots.Add(plot);
        }

        // Nothing to sabotage
        if (plantedPlots.Count == 0)
        {
            Debug.Log("[CanonFire] No planted plots found.");
            return;
        }

        // Choose one planted plot randomly
        int randomIndex = Random.Range(0, plantedPlots.Count);

        plantedPlots[randomIndex].CommitAction(action);
    }

    // LOCAL -> REMOTE

    private void SendSabotage(string action)
    {
        // Fire MY cannon
        if (canonAnim != null)
            canonAnim.SetTrigger("_IsFiring");

        AudioManager.instance.Play("CanonSFX1");

        CommitAnimations(0);

        // Tell opponent to fire THEIR cannon
        photonView.RPC(
            nameof(RPC_ApplySabotage),
            RpcTarget.Others,
            action);
    }

    // ITEMS

    public void GiveBugs()
    {
        SendSabotage("GETBUGGED");
    }

    public void GetWaterLogged()
    {
        SendSabotage("GetWaterLogged");
    }

    public void SOILEDIT()
    {
        SendSabotage("UnTillable");
    }

    public void RemoveLePlants()
    {
        SendSabotage("RemovePlants");
    }

    public void GetOld()
    {
        SendSabotage("FERTILIZING");
    }
}