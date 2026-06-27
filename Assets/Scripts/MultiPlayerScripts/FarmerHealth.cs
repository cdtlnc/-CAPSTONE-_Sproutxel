using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class FarmerHealth : MonoBehaviourPun
{
    [Header("Farmer ")]
    [SerializeField] public int Health;
    [SerializeField] public Slider HealthSlider;
    [SerializeField] public GoalManager_Multiplayer GoalManagerMulti;
    [SerializeField] public int damagedealt;
    [SerializeField] public string Name;

    void Start()
    {
        HealthSlider.maxValue = Health;
        HealthSlider.value = Health;
        HealthSlider.minValue = 0;

        // Grab the network nickname if connected online, otherwise default to the tag
        if (PhotonNetwork.InRoom)
        {
            Name = PhotonNetwork.NickName;
        }
        else
        {
            Name = gameObject.tag;
        }

        UpdateUI();
    }

    // This is the core logic that now syncs over the network!
    public void DepleteHealth(int damage)
    {
        // We use photonView.RPC to ensure health drops on BOTH players' screens simultaneously
        photonView.RPC("RPC_DepleteHealth", RpcTarget.All, damage);
    }

    [PunRPC]
    private void RPC_DepleteHealth(int damage)
    {
        Health -= damage;
        Debug.Log("[MULTIPLAYER] DEPLETING HEALTH. Current Health: " + Health + " Damage: " + damage);
        Checkhealth();
        UpdateUI();
    }

    public void Checkhealth()
    {
        if (Health <= 0)
        {
            // Only let the master server client declare the game over to prevent conflicts
            if (PhotonNetwork.IsMasterClient)
            {
                // Pass our synchronized Name variable instead of the generic lowercase scene name
                GoalManagerMulti.LoseGame(Name);
            }
        }
    }

    public void UpdateUI()
    {
        if (HealthSlider != null)
        {
            HealthSlider.value = Health;
        }
    }
}