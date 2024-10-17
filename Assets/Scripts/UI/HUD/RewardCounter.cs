using TMPro;
using UnityEngine;
using Zenject;

public class RewardCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    private int _points; 

    [Inject]
    private void Construct(LevelManager levelManager)
    {
        gameObject.SetActive(false);
        levelManager.SubscribeToRoundStart(RoundStart);
        ResetPoints();
    }

    private void RoundStart(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundEnd(RoundEnd);
        ResetPoints();
        gameObject.SetActive(true);
    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
    }

    public void IncreasePointsPerRound(int increaseValue)
    {
        _points += increaseValue;
        UpdateText();
    }

    private void UpdateText()
    {
        _text.text = _points.ToString();
    }

    private void ResetPoints()
    {
        _points = 0;
        UpdateText();
    }

}
