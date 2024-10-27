using TMPro;
using UnityEngine;
using Zenject;

public class RewardCounter : MonoBehaviour
{
    [SerializeField] SoundPlayerIncreasePitch _soundPlayer;
    [SerializeField] TextMeshProUGUI _text;

    int _points;

    [Inject]
    private void Construct(LevelManager levelManager, AudioSourcePool audioSource)
    {
        _soundPlayer.Initialize(audioSource);
        levelManager.SubscribeToRoundStart(RoundStart);
    }

    private void RoundStart(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundEnd(RoundEnd);
    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
    }

    public void IncreasePointsPerRound(int increaseValue)
    {
        _points += increaseValue;

        _soundPlayer.PlaySound();

        UpdateText();
    }

    private void UpdateText()
    {
        _text.text = _points.ToString();
    }

    private void OnDestroy()
    {
        _soundPlayer.Dispose();
    }

}

