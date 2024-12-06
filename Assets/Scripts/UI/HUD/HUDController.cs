using Core;
using UnityEngine;
using Zenject;

public class HUDManager : MonoBehaviour
{
    [SerializeField] GameObject[] _allHudElements;
    [SerializeField] GameObject[] _elementsOnActiveForRound;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:������� �������������� �������� �����", Justification = "<��������>")]
    private void Construct(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
        SetHUDActive(false);
    }

    private void Start()
    {
        AdjustForSafeArea();
    }

    private void RoundStart(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundEnd(RoundEnd);
        SetHUDActive(true);
    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
        SetHUDActive(false);
    }

    private void SetHUDActive(bool isActive)
    {
        foreach (var element in _elementsOnActiveForRound)
        {
            element.SetActive(isActive);
        }
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
