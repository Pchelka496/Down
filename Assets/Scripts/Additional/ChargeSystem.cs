using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Additional;

public class ChargeSystem : IDisposable
{
    int _maxCharges;
    float _chargeCooldown;
    int _currentCharges;
    bool _isRecharging;

    CancellationTokenSource _cts;

    event Action<int> OnChargeChanged;

    public void Initialize(int maxCharges, float chargeCooldown, bool charged = true)
    {
        _maxCharges = maxCharges;
        _chargeCooldown = chargeCooldown;

        if (charged)
        {
            _currentCharges = _maxCharges;
        }
        else
        {
            _currentCharges = 0;
        }

    }

    public bool HasCharge => _currentCharges > 0;

    public bool UseCharge()
    {
        if (_currentCharges > 0)
        {
            _currentCharges--;
            OnChargeChanged?.Invoke(_currentCharges);

            if (_currentCharges < _maxCharges && !_isRecharging)
            {
                StartRecharge().Forget();
            }
            return true;
        }
        return false;
    }

    private async UniTaskVoid StartRecharge()
    {
        _isRecharging = true;

        while (_currentCharges < _maxCharges)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_chargeCooldown));
            _currentCharges++;
            OnChargeChanged?.Invoke(_currentCharges);
        }

        _isRecharging = false;
    }

    public void CancelRecharge()
    {
        ClearToken();

        _isRecharging = false;
    }

    public void SubscribeToChargeChange(Action<int> listener)
    {
        OnChargeChanged += listener;
    }

    public void UnsubscribeFromChargeChange(Action<int> listener)
    {
        OnChargeChanged -= listener;
    }

    private void ClearToken() => ClearTokenSupport.ClearToken(ref _cts);

    public void Dispose()
    {
        ClearToken();
    }

}
