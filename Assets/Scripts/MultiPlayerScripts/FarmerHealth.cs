using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class FarmerHealth : MonoBehaviourPun
{
    [Header("Farmer")]
    [SerializeField] public int Health;
    [SerializeField] public Slider HealthSlider;
    [SerializeField] public string Name;

    private int _maxHealth;

    private void Start()
    {
        _maxHealth = Health;

        if (HealthSlider != null)
        {
            HealthSlider.maxValue = Health;
            HealthSlider.value = Health;
            HealthSlider.minValue = 0;
        }

        Name = gameObject.tag;

        if (PhotonNetwork.IsConnected)
            StartCoroutine(SendInitialHealthAfterDelay());
    }

    private IEnumerator SendInitialHealthAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);

        GhostFarmView ghost = FindFirstObjectByType<GhostFarmView>();

        if (ghost == null)
        {
            Debug.LogWarning("[FarmerHealth] GhostFarmView not found.");
            yield break;
        }

        if (ghost.photonView == null)
        {
            Debug.LogWarning("[FarmerHealth] GhostFarmView has no PhotonView.");
            yield break;
        }

        ghost.photonView.RPC(
            "RPC_InitOpponentHealth",
            RpcTarget.Others,
            (float)_maxHealth,
            (float)Health);
    }

    [PunRPC]
    public void RPC_TakeDamage(int damage)
    {
        Health -= damage;

        Debug.Log($"[FarmerHealth] Took {damage} damage. Remaining: {Health}");

        if (HealthSlider != null)
            HealthSlider.value = Health;

        GhostFarmView ghost = FindFirstObjectByType<GhostFarmView>();

        if (ghost != null && ghost.photonView != null)
        {
            ghost.photonView.RPC(
                "RPC_UpdateOpponentHealth",
                RpcTarget.Others,
                (float)Health);
        }

        CheckHealth();
    }

    private void CheckHealth()
    {
        if (Health > 0)
            return;

        Debug.Log($"{Name} has been defeated.");

        WinOrLoseManager_Multiplayer manager =
            FindFirstObjectByType<WinOrLoseManager_Multiplayer>();

        if (manager != null)
            manager.ShowLoser();

        photonView.RPC(nameof(RPC_ShowWinner), RpcTarget.Others);
    }

    [PunRPC]
    private void RPC_ShowWinner()
    {
        WinOrLoseManager_Multiplayer manager =
            FindFirstObjectByType<WinOrLoseManager_Multiplayer>();

        if (manager != null)
            manager.ShowWinner();
    }
}