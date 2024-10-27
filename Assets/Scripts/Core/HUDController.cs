using UnityEngine;
using Zenject;

public class HUDManager : MonoBehaviour
{
    [SerializeField] GameObject[] _hudElements; 

    [Inject]
    private void Construct(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
        levelManager.SubscribeToRoundEnd(RoundEnd);

        SetHUDActive(false);
    }

    private void RoundStart(LevelManager levelManager)
    {
        SetHUDActive(true);
    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
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
