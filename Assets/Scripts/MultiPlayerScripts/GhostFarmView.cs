using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class GhostFarmView : MonoBehaviourPun
{
    [SerializeField] private GhostPlotManager plot0;
    [SerializeField] private GhostPlotManager plot1;
    [SerializeField] private GhostPlotManager plot2;

    [Header("Opponent health bar")]
    [SerializeField] private Slider opponentHealthSlider;
    [SerializeField] private int opponentStartingHealth = 100;

    private GhostPlotManager[] _plots;

    private void Awake()
    {
        _plots = new GhostPlotManager[] { plot0, plot1, plot2 };
    }

    private void Start()
    {
        if (opponentHealthSlider != null)
        {
            opponentHealthSlider.minValue = 0;
            opponentHealthSlider.maxValue = opponentStartingHealth;
            opponentHealthSlider.value = opponentStartingHealth;
        }
    }

    // RPCs

    [PunRPC]
    public void RPC_UpdateGhostPlot(int plotIndex, string spriteName)
    {
        if (plotIndex < 0 || plotIndex >= _plots.Length) return;
        GhostPlotManager target = _plots[plotIndex];
        if (target == null) return;

        Sprite sprite = string.IsNullOrEmpty(spriteName)
            ? null
            : SpriteRegistry.Get(spriteName);

        target.SetSprite(sprite);
    }

    [PunRPC]
    public void RPC_InitOpponentHealth(float maxHealth, float currentHealth)
    {
        if (opponentHealthSlider == null) return;
        opponentHealthSlider.maxValue = maxHealth;
        opponentHealthSlider.value = currentHealth;
    }

    [PunRPC]
    public void RPC_UpdateOpponentHealth(float currentHealth)
    {
        if (opponentHealthSlider != null)
            opponentHealthSlider.value = currentHealth;
    }

    // called when opponent tills plot
    [PunRPC]
    public void RPC_SetGhostTilled(int plotIndex, bool tilled)
    {
        if (plotIndex < 0 || plotIndex >= _plots.Length) return;
        _plots[plotIndex]?.SetTilled(tilled);
    }

    // called when opponent plot is waterlogged
    [PunRPC]
    public void RPC_SetGhostWaterlogged(int plotIndex, bool waterlogged)
    {
        if (plotIndex < 0 || plotIndex >= _plots.Length) return;
        _plots[plotIndex]?.SetWaterlogged(waterlogged);
    }

    // called when opponent plot gets infested
    [PunRPC]
    public void RPC_SetGhostBugged(int plotIndex, bool bugged)
    {
        if (plotIndex < 0 || plotIndex >= _plots.Length) return;
        _plots[plotIndex]?.SetBugged(bugged);
    }
}