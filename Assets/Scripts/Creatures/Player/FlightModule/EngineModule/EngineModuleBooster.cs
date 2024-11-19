using System;
using UnityEngine;

public class EngineModuleBooster : IDisposable
{
    float _boosterForce;

    Transform _engine;
    Rigidbody2D _rb;
    ChargeSystem _chargeSystem = new();
    EngineModuleVisualPart _visualPart;

    public void Initialize(Rigidbody2D rb, Transform engine, EngineModuleVisualPart visualPart)
    {
        _rb = rb;
        _engine = engine;
        _visualPart = visualPart;
    }

    public void UpdateCharacteristics(float boosterForce, int chargeCount, float chargeCooldown)
    {
        _boosterForce = boosterForce;
        _chargeSystem.Initialize(chargeCount, chargeCooldown);
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
    }

}
