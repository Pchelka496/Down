using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using Zenject;

public class RewardCounter : MonoBehaviour
{
    [SerializeField] SoundPlayerIncreasePitch _soundPlayer;
    [SerializeField] TextMeshProUGUI _text;
    PickUpItemManager _pickUpItemManager;

    float _pickUpRewardMultiplier = 1f;
    int _points;

    public float PickUpRewardMultiplier { set => _pickUpRewardMultiplier = value; }

    [Inject]
    private void Construct(LevelManager levelManager, PickUpItemManager pickUpItemManager, AudioSourcePool audioSource)
    {
        _soundPlayer.Initialize(audioSource);
        _pickUpItemManager = pickUpItemManager;
        ResetPoints();

        levelManager.SubscribeToRoundStart(RoundStart);
    }

    private void RoundStart(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundEnd(RoundEnd);
    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        levelManager.SubscribeToRoundStart(RoundStart);

        _pickUpItemManager.IncreasePoints(_points);
        ResetPoints();
    }

    private void ResetPoints()
    {
        _points = 0;
        UpdateText();
    }

    public void IncreasePointsPerRound(int increaseValue)
    {
        _points = _points + (int)(increaseValue * _pickUpRewardMultiplier);

        _soundPlayer.PlaySound();

        UpdateText();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateText()
    {
        _text.text = _points.ToString();
    }

    private void OnDestroy()
    {
        _soundPlayer.Dispose();
    }

}

