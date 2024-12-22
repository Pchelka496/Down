using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using Zenject;

public class RewardCounter : MonoBehaviour, ICollectedMoneyTracker
{
    [SerializeField] SoundPlayerIncreasePitch[] _soundPlayers;
    [SerializeField] TextMeshProUGUI _text;

    PlayerResourcedKeeper _rewardKeeper;

    float _pickUpRewardMultiplier = 1f;
    int _points;
    int _currentSoundPlayerIndex = 0; 

    event System.Action DisposeEvents;
    event System.Action<int> OnMoneyChanged;

    public float PickUpRewardMultiplier { set => _pickUpRewardMultiplier = value; }

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(GlobalEventsManager globalEventsManager, PlayerResourcedKeeper rewardKeeper, AudioSourcePool audioSource)
    {
        foreach (var soundPlayer in _soundPlayers)
        {
            soundPlayer.Initialize(audioSource);
            soundPlayer.SubscribeToOnPitchReset(SwitchToNextSoundPlayer);
        }

        _rewardKeeper = rewardKeeper;
        ResetPoints();

        globalEventsManager.SubscribeToRoundStarted(RoundStart);
        globalEventsManager.SubscribeToFastTravelStarted(FastTravelStart);
        globalEventsManager.SubscribeToRoundEnded(RoundEnd);

        DisposeEvents += () => globalEventsManager?.UnsubscribeFromFastTravelStarted(FastTravelStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);
    }

    private void Start()
    {
        RoundEnd();
    }

    private void FastTravelStart() => gameObject.SetActive(true);

    private void RoundStart()
    {
        ResetPoints();
        gameObject.SetActive(true);
    }

    private void RoundEnd()
    {
        gameObject.SetActive(false);

        _rewardKeeper.IncreaseMoney(_points);
    }

    private void ResetPoints()
    {
        _points = 0;
        UpdateText();
    }

    public void IncreasePointsPerRound(int increaseValue)
    {
        _points += (int)(increaseValue * _pickUpRewardMultiplier);

        _soundPlayers[_currentSoundPlayerIndex].PlaySound();

        UpdateText();

        OnMoneyChanged?.Invoke(_points);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateText()
    {
        _text.text = _points.ToString();
    }

    private void SwitchToNextSoundPlayer()
    {
        _currentSoundPlayerIndex = (_currentSoundPlayerIndex + 1) % _soundPlayers.Length; 
    }

    int ICollectedMoneyTracker.GetCollectedMoney() => _points;
    void ICollectedMoneyTracker.SubscribeToMoneyChanged(System.Action<int> callback) => OnMoneyChanged += callback;
    void ICollectedMoneyTracker.UnsubscribeFromMoneyChanged(System.Action<int> callback) => OnMoneyChanged -= callback;

    private void OnDestroy()
    {
        foreach (var soundPlayer in _soundPlayers)
        {
            soundPlayer.UnsubscribeToOnPitchReset(SwitchToNextSoundPlayer);
            soundPlayer.Dispose();
        }

        DisposeEvents?.Invoke();
    }
}
