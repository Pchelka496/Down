using System;
using UnityEngine;

public class EngineModuleBooster : IDisposable
{
    float _boosterForce;

    readonly ChargeSystem _chargeSystem = new();
    Transform _engine;
    Rigidbody2D _rb;
    EngineModuleVisualPart _visualPart;
    BoosterIndicator _indicator;

    public void Initialize(Rigidbody2D rb, Transform engine, EngineModuleVisualPart visualPart, BoosterIndicator indicator)
    {
        _rb = rb;
        _engine = engine;
        _visualPart = visualPart;
        _indicator = indicator;
    }

    public void UpdateCharacteristics(float boosterForce, int chargeCount, float chargeCooldown)
    {
        _boosterForce = boosterForce;
        _chargeSystem.Initialize(chargeCount, chargeCooldown, true);

        _indicator.UpdateMaxChargeAmount(chargeCount);
        _indicator.UpdateCurrentChargeAmount(chargeCount);
        _chargeSystem.SubscribeToChargeChange(_indicator.UpdateCurrentChargeAmount);
    }

    public void ApplyBoost()
    {
        if (_chargeSystem.UseCharge())
        {
            EngineModule.ApplyForce(_rb, _engine, _boosterForce);
            _visualPart.Boost();
        }
    }

    public void Dispose()
    {
        _chargeSystem.Dispose();
        _chargeSystem.UnsubscribeFromChargeChange(_indicator.UpdateCurrentChargeAmount);
    }

}
