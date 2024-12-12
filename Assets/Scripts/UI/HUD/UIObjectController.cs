using UnityEngine;

public class UIObjectController : MonoBehaviour
{
    [SerializeField] RectTransform[] _allHudElements;

    private void Start()
    {
        AdjustForSafeArea();
    }

    private void AdjustForSafeArea()
    {
        Rect safeArea = Screen.safeArea;

        foreach (var element in _allHudElements)
        {
            if (!element.TryGetComponent<RectTransform>(out var rectTransform)) continue;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }
    }
}
