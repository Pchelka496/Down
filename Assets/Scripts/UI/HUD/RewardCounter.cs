using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using Zenject;

public class RewardCounter : MonoBehaviour, ICollectedMoneyTracker
{
    [SerializeField] SoundPlayerIncreasePitch _soundPlayer;
    [SerializeField] TextMeshProUGUI _text;

    PlayerResourcedKeeper _rewardKeeper;

    float _pickUpRewardMultiplier = 1f;
    int _points;
    event System.Action DisposeEvents;
    event System.Action<int> OnMoneyChanged;

    public float PickUpRewardMultiplier { set => _pickUpRewardMultiplier = value; }

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(GlobalEventsManager globalEventsManager, PlayerResourcedKeeper rewardKeeper, AudioSourcePool audioSource)
    {
        _soundPlayer.Initialize(audioSource);
        _rewardKeeper = rewardKeeper;
        ResetPoints();

        globalEventsManager.SubscribeToRoundStarted(RoundStart);
        globalEventsManager.SubscribeToWarpStarted(WarpStart);
        globalEventsManager.SubscribeToRoundEnded(RoundEnd);

        DisposeEvents += () => globalEventsManager?.UnsubscribeFromWarpStarted(WarpStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);
    }

    private void Start()
    {
        RoundEnd();
    }

    private void WarpStart() => gameObject.SetActive(true);

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

        _soundPlayer.PlaySound();

        UpdateText();

        OnMoneyChanged?.Invoke(_points);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateText()
    {
        _text.text = _points.ToString();
    }

    int ICollectedMoneyTracker.GetCollectedMoney() => _points;
    void ICollectedMoneyTracker.SubscribeToMoneyChanged(System.Action<int> callback) => OnMoneyChanged += callback;
    void ICollectedMoneyTracker.UnsubscribeFromMoneyChanged(System.Action<int> callback) => OnMoneyChanged -= callback;

    private void OnDestroy()
    {
        _soundPlayer.Dispose();
        DisposeEvents?.Invoke();
    }
}
