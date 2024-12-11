using TMPro;
using UnityEngine;
using System.Threading;
using System.Runtime.CompilerServices;
using Additional;
using Zenject;

public class HealthIndicator : MonoBehaviour
{
    const string SUFFIX = "%";
    const float TEXT_UPDATE_DELAY = 0.05f;

    [SerializeField] RectTransform _defaultIconPosition;
    [SerializeField] TextMeshProUGUI _healthText;
    [SerializeField] Gradient _healthGradient;

    int _maxHealth;
    int _currentHealth;

    event System.Action DisposeEvents;
    CancellationTokenSource _cts;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(GlobalEventsManager globalEventsManager)
    {
        globalEventsManager.SubscribeToRoundStarted(RoundStart);

        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
    }

    private void RoundStart()
    {
        SetDefaultIndicatorPosition();
        gameObject.SetActive(true);
    }

    public void Initialize(int maxHealth, int currentHealth)
    {
        _maxHealth = maxHealth;
        UpdateHealth(currentHealth);
        SetDefaultIndicatorPosition();
    }

    public void UpdateHealth(int currentHealth)
    {
        _currentHealth = Mathf.Clamp(currentHealth, 0, _maxHealth);
        UpdateHealthTextSmooth(_currentHealth);
    }

    public void SetNewIndicatorPosition(Vector2 position)
    {
        _healthText.transform.position = position;
    }

    public void SetDefaultIndicatorPosition()
    {
        _healthText.transform.position = _defaultIconPosition.position;
    }

    private void UpdateHealthTextSmooth(int targetHealth)
    {
        ClearToken();
        _cts = new CancellationTokenSource();

        var targetPercentage = Mathf.RoundToInt((float)targetHealth / _maxHealth * 100);

        _healthText.SmoothUpdateText(targetValue: targetPercentage,
                                     token: _cts.Token,
                                     textUpdateDelay: TEXT_UPDATE_DELAY,
                                     suffix: SUFFIX,
                                     onValueUpdate: UpdateHealthTextColor
                                     ).Forget();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateHealthTextColor(int healthValue)
    {
        float healthPercentage = (float)healthValue / _maxHealth;
        _healthText.color = _healthGradient.Evaluate(healthPercentage);
    }

    private void ClearToken() => ClearTokenSupport.ClearToken(ref _cts);

    private void OnDestroy()
    {
        ClearToken();
        DisposeEvents?.Invoke();
    }
}


