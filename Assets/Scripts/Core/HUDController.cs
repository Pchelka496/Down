using UnityEngine;
using Zenject;

public class HUDManager : MonoBehaviour
{
    [Header("Elements for switching off in lobby mode")]
    [SerializeField] GameObject[] _hudElements; 

    [Inject]
    private void Construct(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundStart(RoundStart);

        SetHUDActive(false);
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
        foreach (var element in _hudElements)
        {
            element.SetActive(isActive);
        }
    }

}
