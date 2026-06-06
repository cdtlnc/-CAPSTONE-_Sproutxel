using UnityEngine;
using UnityEngine.UI;
public class HeatDazeScript : MonoBehaviour
{
    [SerializeField] private Sprite[] rainFrames;
    [SerializeField] private float fps = 12f;
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private float maxAlpha = 0.7f;
    [SerializeField] private float minAlpha = 0.3f;

    private Image _spriteRenderer;
    private Image _image;


    private int _currentFrame = 0;
    private float _frameTimer = 0f;
    private float _currentAlpha = 0f;
    private bool _shouldShow = false;

    private void Awake()
    {
        _spriteRenderer = GetComponent<Image>();
        SetAlpha(0f);
    }

    private void OnEnable()
    {
        TickManager.OnEventTick += OnEventTick;
    }

    private void OnDisable()
    {
        TickManager.OnEventTick -= OnEventTick;
    }

    private void OnEventTick(object sender, TickManager.OnTickEventArgs e)
    {
        _shouldShow = EventManager._weatherEvent == 2;
    }

    private void Update()
    {
      
        float t = Mathf.PingPong(Time.time * fadeSpeed, 1f);

       
        _currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, t);
        SetAlpha(_currentAlpha);

     
    }

    private void SetAlpha(float alpha)
    {
        if (_spriteRenderer != null)
        {
            Color c = _spriteRenderer.color;
            c.a = alpha;
            _spriteRenderer.color = c;
        }
        else if (_image != null)
        {
            Color c = _image.color;
            c.a = alpha;
            _image.color = c;
        }
    }


    public void DisableHeatDaze()
    {
        Debug.Log("Disabling heatDaze panel");
        transform.parent.gameObject.SetActive(false);
    }

  
}
