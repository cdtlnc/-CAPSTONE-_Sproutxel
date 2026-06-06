using UnityEngine;
using UnityEngine.UI;

public class RainScript : MonoBehaviour
{
    [SerializeField] private Sprite[] rainFrames;
    [SerializeField] private float fps = 12f;
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private float maxAlpha = 0.7f;

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
        
        float targetAlpha = _shouldShow ? maxAlpha : 0f;
        _currentAlpha = Mathf.MoveTowards(_currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
        SetAlpha(_currentAlpha);

        if (_currentAlpha <= 0f || rainFrames == null || rainFrames.Length == 0) return;

        _frameTimer += Time.deltaTime;
        if (_frameTimer >= 1f / fps)
        {
            _frameTimer -= 1f / fps;
            _currentFrame = (_currentFrame + 1) % rainFrames.Length;
            SetSprite(rainFrames[_currentFrame]);
        }
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

    private void SetSprite(Sprite sprite)
    {
        if (_spriteRenderer != null)
            _spriteRenderer.sprite = sprite;
        else if (_image != null)
            _image.sprite = sprite;
    }

    public void DisableTyphoone()
    {
        Debug.Log("Disabling RainScript panel");
        transform.parent.gameObject.SetActive(false);
    }
}