using System;
using System.Threading;
using Additional;
using UnityEngine;

public class ExchangePanel : MonoBehaviour, IUIPanel
{
    [Header("Exchange Buttons")]
    [SerializeField] CustomPressButton[] _buttons;

    [Header("Exchange Rates")]
    [SerializeField] int _adRewardDiamonds = 5;
    [SerializeField] float _diamondsToMoneyRate = 100f;
    [SerializeField] float _diamondsToEnergyRate = 5f;
    [SerializeField] float _moneyToDiamondsRate = 0.005f;
    [SerializeField] float _moneyToEnergyRate = 0.02f;

    [Header("Sounds")]
    [SerializeField] SoundPlayerRandomPitch _successSoundPlayer;
    [SerializeField] SoundPlayerRandomPitch _unsuccessfulSoundPlayer;

    PlayerResourcedKeeper _playerResourcedKeeper;
    CancellationTokenSource _cts;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(PlayerResourcedKeeper resourcedKeeper, AudioSourcePool audioSourcePool)
    {
        _playerResourcedKeeper = resourcedKeeper;
        _successSoundPlayer.Initialize(audioSourcePool);
        _unsuccessfulSoundPlayer.Initialize(audioSourcePool);
    }

    public void Open()
    {
        transform.SetAsLastSibling();
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        ClearToken();
    }

    public void ExchangeMoneyToDiamonds()
        => Exchange(_playerResourcedKeeper.TryDecreaseMoney, _playerResourcedKeeper.IncreaseDiamonds, _moneyToDiamondsRate);

    public void ExchangeMoneyToEnergy()
        => Exchange(_playerResourcedKeeper.TryDecreaseMoney, _playerResourcedKeeper.IncreaseEnergy, _moneyToEnergyRate);

    public void ExchangeDiamondsToMoney()
        => Exchange(_playerResourcedKeeper.TryDecreaseDiamonds, _playerResourcedKeeper.IncreaseMoney, _diamondsToMoneyRate);

    public void ExchangeDiamondsToEnergy()
        => Exchange(_playerResourcedKeeper.TryDecreaseDiamonds, _playerResourcedKeeper.IncreaseEnergy, _diamondsToEnergyRate);

    private void Exchange(Func<int, bool, bool> tryDecreaseFunc, Action<int> increaseAction, float exchangeRate)
    {
        int minDecreaseAmount = Mathf.CeilToInt(1 / exchangeRate);

        if (tryDecreaseFunc.Invoke(minDecreaseAmount, true))
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
        _playerResourcedKeeper.IncreaseDiamonds(_adRewardDiamonds);
        _successSoundPlayer.PlayNextSound();
    }

    private void ClearToken() => ClearTokenSupport.ClearToken(ref _cts);

    private void OnDestroy()
    {
        ClearToken();
    }
}
