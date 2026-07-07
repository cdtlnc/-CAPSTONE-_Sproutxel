using UnityEngine;

public class SafeAreaHandler : MonoBehaviour
{
    private RectTransform _panel;
    private Rect _lastSafeArea = Rect.zero;
    private Vector2Int _lastScreenSize;

    private void Awake()
    {
        _panel = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    private void Update()
    {
        if (_lastSafeArea != Screen.safeArea ||
            _lastScreenSize != new Vector2Int(Screen.width, Screen.height))
        {
            ApplySafeArea();
        }
    }

    private void ApplySafeArea()
    {
        _lastSafeArea = Screen.safeArea;
        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

        Rect safeArea = Screen.safeArea;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        _panel.anchorMin = anchorMin;
        _panel.anchorMax = anchorMax;
    }
}