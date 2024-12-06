using System;
using System.Threading;
using Additional;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ExchangePanel : MonoBehaviour, IUIPanel
{
    [Header("Exchange Buttons")]
    [SerializeField] ExchangeButtonContainer[] _buttons;

    [Header("Exchange Rates")]
    [SerializeField] float _moneyToDiamondsRate = 10f;
    [SerializeField] float _moneyToEnergyRate = 5f;
    [SerializeField] float _diamondsToMoneyRate = 0.1f;
    [SerializeField] float _diamondsToEnergyRate = 0.2f;
    [SerializeField] int _adRewardDiamonds = 5;

    [Header("Sounds")]
    [SerializeField] SoundPlayerRandomPitch _successSoundPlayer;
    [SerializeField] SoundPlayerRandomPitch _unsuccessfulSoundPlayer;

    [Header("Task Settings")]
    [SerializeField] float _buttonCheckFrequency = 0.2f;

    RewardKeeper _rewardKeeper;
    CancellationTokenSource _cts;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(RewardKeeper rewardKeeper, AudioSourcePool audioSourcePool)
    {
        _rewardKeeper = rewardKeeper;
        _successSoundPlayer.Initialize(audioSourcePool);
        _unsuccessfulSoundPlayer.Initialize(audioSourcePool);
    }

    private void Start()
    {
        foreach (var buttonContainer in _buttons)
        {
            buttonContainer.SetOnClickAction();
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
        ClearToken();
        _cts = new();
        MonitorButtonClicks(_cts.Token).Forget();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        ClearToken();
    }

    private async UniTaskVoid MonitorButtonClicks(CancellationToken token)
    {
        var delaySpan = TimeSpan.FromSeconds(_buttonCheckFrequency);

        while (!token.IsCancellationRequested)
        {
            foreach (var buttonContainer in _buttons)
            {
                buttonContainer.CheckAndClick();
            }

            await UniTask.Delay(delaySpan, cancellationToken: token);
        }
    }

    public void ExchangeMoneyToDiamonds() => Exchange(_rewardKeeper.TryDecreaseMoney, _rewardKeeper.IncreaseDiamonds, _moneyToDiamondsRate);
    public void ExchangeMoneyToEnergy() => Exchange(_rewardKeeper.TryDecreaseMoney, _rewardKeeper.IncreaseEnergy, _moneyToEnergyRate);
    public void ExchangeDiamondsToMoney() => Exchange(_rewardKeeper.TryDecreaseDiamonds, _rewardKeeper.IncreaseMoney, _diamondsToMoneyRate);
    public void ExchangeDiamondsToEnergy() => Exchange(_rewardKeeper.TryDecreaseDiamonds, _rewardKeeper.IncreaseEnergy, _diamondsToEnergyRate);

    private void Exchange(Func<int, bool> tryDecreaseFunc, Action<int> increaseAction, float exchangeRate)
    {
        int minDecreaseAmount = Mathf.CeilToInt(1 / exchangeRate);

        if (tryDecreaseFunc.Invoke(minDecreaseAmount))
        {
            int increaseAmount = Mathf.FloorToInt(minDecreaseAmount * exchangeRate);
            increaseAction.Invoke(increaseAmount);

            _successSoundPlayer.PlayNextSound();
        }
        else
        {
            _unsuccessfulSoundPlayer.PlayNextSound();
        }
    }

    public void WatchAdForDiamonds()
    {
        OnAdWatchedSuccessfully();
    }

    private void OnAdWatchedSuccessfully()
    {
        _rewardKeeper.IncreaseDiamonds(_adRewardDiamonds);
        _successSoundPlayer.PlayNextSound();
    }

    private void ClearToken() => ClearTokenSupport.ClearToken(ref _cts);

    private void OnDestroy()
    {
        ClearToken();
    }

    [System.Serializable]
    private class ExchangeButtonContainer
    {
        [SerializeField] Button _button;
        [Header("This action will be added in onClick")]
        [SerializeField] UnityEvent _onClickEvent;

        public void SetOnClickAction()
        {
            _button.onClick.AddListener(() => _onClickEvent.Invoke());
        }

        public void CheckAndClick()
        {
            if (_button != null && _button.interactable)
            {
                _onClickEvent.Invoke();
            }
        }
    }
}
