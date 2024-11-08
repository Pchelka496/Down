using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using System.Threading;
using System;

public class BoosterModul : BaseModule
{
    [SerializeField] float _boostForce;
    [SerializeField] float _colliderDisableDuration = 0.2f;
    [SerializeField] int _maxCharges = 3;
    [SerializeField] float _chargeCooldown = 3f;

    Controls _controls;
    EmergencyBrakeModule _emergencyBrakeModule;
    BoosterIndicator _indicator;
    BoosterModuleConfig _config;
    Rigidbody2D _rb;
    Collider2D _collider;
    Transform _transform;
    int _currentCharges;
    bool _isRecharging;

    CancellationTokenSource _cts;

    [Inject]
    private void Construct(CharacterController player, BoosterModuleConfig config, Controls controls, LevelManager levelManager, BoosterIndicator boosterIndicator)
    {
        _config = config;
        _rb = player.Rb;
        _collider = player.Collider;
        _transform = player.transform;
        _boostForce = _config.GetBoosterPower();
        _controls = controls;
        _indicator = boosterIndicator;
        _currentCharges = _maxCharges;

        UpdateIndicator();

        if (levelManager.IsRoundActive)
        {
            RoundStart(levelManager);
        }
        else
        {
            levelManager.SubscribeToRoundStart(RoundStart);
        }

        SnapToPlayer(player.transform);
    }

    private void RoundStart(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundEnd(RoundEnd);

        _emergencyBrakeModule = GameplaySceneInstaller.DiContainer.Resolve<CharacterController>().EmergencyBrakeModule;

        _controls.Player.LeftBooster.performed += ctx => TryBoost(Vector2.left);
        _controls.Player.RightBooster.performed += ctx => TryBoost(Vector2.right);
        _controls.Player.UpBooster.performed += ctx => TryBoost(Vector2.up);
        _controls.Player.DownBooster.performed += ctx => TryBoost(Vector2.down);
    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        levelManager.SubscribeToRoundStart(RoundStart);

        _controls.Player.LeftBooster.performed -= ctx => TryBoost(Vector2.left);
        _controls.Player.RightBooster.performed -= ctx => TryBoost(Vector2.right);
        _controls.Player.UpBooster.performed -= ctx => TryBoost(Vector2.up);
        _controls.Player.DownBooster.performed -= ctx => TryBoost(Vector2.down);
    }

    private void TryBoost(Vector2 direction)
    {
        if (_currentCharges > 0)
        {
            Boost(direction);
            _currentCharges--;

            UpdateIndicator();

            if (_currentCharges < _maxCharges && !_isRecharging)
            {
                StartRecharge().Forget();
            }
        }
    }

    private async UniTaskVoid StartRecharge()
    {
        _isRecharging = true;

        while (_currentCharges < _maxCharges)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_chargeCooldown));
            _currentCharges++;
            UpdateIndicator();
        }

        _isRecharging = false;
    }

    private void UpdateIndicator()
    {
        if (_currentCharges > 0)
        {
            _indicator.SetReadyStatus();
        }
        else
        {
            _indicator.SetUnreadyStatus();
        }
    }

    private void Boost(Vector2 direction)
    {
        _rb.AddForce(direction * _boostForce, ForceMode2D.Impulse);
        ActiveDisableCollider();
    }

    private void ActiveDisableCollider()
    {
        ClearToken(ref _cts);

        _cts = new CancellationTokenSource();

        DisableColliderTemporarily(_cts.Token).Forget();
    }

    private async UniTaskVoid DisableColliderTemporarily(CancellationToken token)
    {
        _collider.enabled = false;
        _emergencyBrakeModule?.DisableModule();

        try
        {
            await UniTask.WaitForSeconds(_colliderDisableDuration, cancellationToken: token);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (!token.IsCancellationRequested)
            {
                _collider.enabled = true;
                _emergencyBrakeModule?.EnableModule();
            }
        }
    }

    private void ClearToken(ref CancellationTokenSource cts)
    {
        if (cts == null) return;

        if (!cts.IsCancellationRequested)
        {
            cts.Cancel();
        }

        cts.Dispose();
        cts = null;
    }

}




