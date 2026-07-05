using UnityEngine;

public class GhostPlotManager : MonoBehaviour
{
    [Header("Plant")]
    [SerializeField] private SpriteRenderer plantRenderer;

    [Header("Soil Tilled visuals")]
    [SerializeField] private GameObject untilledVisual;
    [SerializeField] private GameObject tilledVisual;

    [Header("Waterlog visual")]
    [SerializeField] private GameObject waterVisual;

    [Header("Bug infestation visual")]
    [SerializeField] private GameObject bugVisual;

    private void Start()
    {
        SetTilled(false);
        SetWaterlogged(false);
        SetBugged(false);
    }

    public void SetSprite(Sprite sprite)
    {
        if (plantRenderer == null) return;
        plantRenderer.sprite = sprite;
        plantRenderer.enabled = sprite != null;
    }

    public void SetTilled(bool tilled)
    {
        if (untilledVisual != null) untilledVisual.SetActive(!tilled);
        if (tilledVisual != null) tilledVisual.SetActive(tilled);
    }

    public void SetWaterlogged(bool waterlogged)
    {
        if (waterVisual != null) waterVisual.SetActive(waterlogged);
    }

    public void SetBugged(bool bugged)
    {
        if (bugVisual != null) bugVisual.SetActive(bugged);
    }
}