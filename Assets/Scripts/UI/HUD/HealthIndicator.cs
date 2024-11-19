using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;
using Zenject;

public class HealthIndicator : MonoBehaviour
{
    const string PERCENT_SYMBOL = "%";
    const float TEXT_UPDATE_DELAY = 0.05f;

    [SerializeField] RectTransform _defaultIconPosition;
    [SerializeField] TextMeshProUGUI _healthText;
    [SerializeField] Gradient _healthGradient;

    int _maxHealth;
    int _currentHealth;

    CancellationTokenSource _cts;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
    }

    private void RoundStart(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundEnd(RoundEnd);
        SetDefaultIndicatorPosition();
        gameObject.SetActive(true);
    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
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
        UpdateHealthTextSmooth(_currentHealth).Forget();
    }

    public void SetNewIndicatorPosition(Vector2 position)
    {
        _healthText.transform.position = position;
    }

    public void SetDefaultIndicatorPosition()
    {
        _healthText.transform.position = _defaultIconPosition.position;
    }

    private async UniTaskVoid UpdateHealthTextSmooth(int targetHealth)
    {
        ClearToken();
        _cts = new CancellationTokenSource();

        var displayedHealth = int.Parse(_healthText.text.Replace(PERCENT_SYMBOL, string.Empty));

        while (displayedHealth != targetHealth)
        {
            displayedHealth += displayedHealth > targetHealth ? -1 : 1;
            _healthText.text = displayedHealth + PERCENT_SYMBOL;

            UpdateHealthTextColor(displayedHealth);

            await UniTask.WaitForSeconds(TEXT_UPDATE_DELAY, cancellationToken: _cts.Token);
        }
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
    }
}


