using UnityEngine;
using Zenject;

public class HealthModule : BaseModule
{
    int _maxHealth;
    int _currentHealth;

  //  [Inject]
    private void Construct(HealthModuleConfig config)
    {
        _maxHealth = config.GetMaxHealth();
    }

}
