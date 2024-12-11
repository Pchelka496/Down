using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using Zenject;

public class RewardCounter : MonoBehaviour
{
    [SerializeField] SoundPlayerIncreasePitch _soundPlayer;
    [SerializeField] TextMeshProUGUI _text;
    PlayerResourcedKeeper _rewardKeeper;

    float _pickUpRewardMultiplier = 1f;
    int _points;
    event System.Action DisposeEvents;

    public float PickUpRewardMultiplier { set => _pickUpRewardMultiplier = value; }

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(GlobalEventsManager globalEventsManager, PlayerResourcedKeeper rewardKeeper, AudioSourcePool audioSource)
    {
        _soundPlayer.Initialize(audioSource);
        _rewardKeeper = rewardKeeper;
        ResetPoints();

        globalEventsManager.SubscribeToRoundEnded(RoundEnd);

        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);
    }

    private void RoundEnd()
    {
        _rewardKeeper.IncreaseMoney(_points);
        ResetPoints();
    }

    private void ResetPoints()
    {
        _points = 0;
        UpdateText();
    }

    public void IncreasePointsPerRound(int increaseValue)
    {
        _points += (int)(increaseValue * _pickUpRewardMultiplier);

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
        DisposeEvents?.Invoke();
    }
}

