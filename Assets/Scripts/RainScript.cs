using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class RainScript : MonoBehaviour
{
    [SerializeField] private Sprite[] rainFrames;
    [SerializeField] private float fps = 12f;
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private float maxAlpha = 0.7f;

    private Image _image;

    private int _currentFrame = 0;
    private float _frameTimer = 0f;
    private float _currentAlpha = 0f;
    private bool _shouldShow = false;

    private void Awake()
    {
        _image = GetComponent<Image>();
        SetAlpha(0f);
    }

    private void OnEnable()
    {
        TickManager.OnEventTick += OnEventTick;
        EvaluateRainVisibility();
    }

    private void OnDisable()
    {
        TickManager.OnEventTick -= OnEventTick;
    }

    private void OnEventTick(object sender, TickManager.OnTickEventArgs e)
    {
        EvaluateRainVisibility();
    }

    private void EvaluateRainVisibility()
    {
        bool isTyphoon;
        bool isWetSeason;

        bool isMultiplayerClient = PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient;

        // reads from NetworkTimeState when running on the multiplayer client device
        // since EventManager and TimeOfDayUI are disabled on the client.
        if (isMultiplayerClient)
        {
            isTyphoon = NetworkTimeState.weatherEvent == 2;
            isWetSeason = !NetworkTimeState.isDrySeason;
        }
        else     // on the host or singleplayer, reads directly from EventManager and TimeOfDayUI.
        {
            isTyphoon = EventManager._weatherEvent == 2;
            isWetSeason = !TimeOfDayUI.isDrySeason;
        }

        _shouldShow = isTyphoon || isWetSeason;
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
        if (_image != null)
        {
            Color c = _image.color;
            c.a = alpha;
            _image.color = c;
        }
    }

    private void SetSprite(Sprite sprite)
    {
        if (_image != null) _image.sprite = sprite;
    }

    public void DisableTyphoone()
    {
        Debug.Log("Disabling RainScript panel");
        transform.parent.gameObject.SetActive(false);
    }
}