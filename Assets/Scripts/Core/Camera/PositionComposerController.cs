using UnityEngine;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;


[System.Serializable]
public class PositionComposerController
{
    [SerializeField] ScreenComposerSettings _defaultComposition;
    [SerializeField] CinemachinePositionComposer _composer;
    [SerializeField] float _maxYScreenPosition = 10f;
    [SerializeField] float _minYScreenPosition = 5f;

    [SerializeField] float _minVelocityThreshold = 0f;
    [SerializeField] float _maxVelocityThreshold = 20f;

    [SerializeField] float _updateFrequency = 1f;
    [SerializeField] AnimationCurve _transitionCurve;

    float _currentYScreenPosition;
    float _velocityYRef;

    Rigidbody2D _playerRb;

    public void Initialize(Rigidbody2D playerRb)
    {
        _playerRb = playerRb;
        StartAdjustingLensSettingsAsync().Forget();
    }

    private async UniTaskVoid StartAdjustingLensSettingsAsync()
    {
        while (true)
        {
            await AdjustLensSettingsBasedOnVelocity();
            await UniTask.WaitForSeconds(_updateFrequency);
        }
    }

    private async UniTask AdjustLensSettingsBasedOnVelocity()
    {
        var velocityY = Mathf.Abs(_playerRb.velocity.y);

        var normalizedVelocity = Mathf.InverseLerp(_minVelocityThreshold, _maxVelocityThreshold, velocityY);
        normalizedVelocity = Mathf.Clamp01(normalizedVelocity); 

        var targetYScreenPosition = Mathf.Lerp(_minYScreenPosition, _maxYScreenPosition, normalizedVelocity);

        var timeElapsed = 0f;
        var duration = 0.5f; 
        var initialYScreenPosition = _currentYScreenPosition;

        while (timeElapsed < duration)
        {
            var curveValue = _transitionCurve.Evaluate(timeElapsed / duration);

            _currentYScreenPosition = Mathf.Lerp(initialYScreenPosition, targetYScreenPosition, curveValue);

            _defaultComposition.ScreenPosition = new(0f, _currentYScreenPosition);
            _composer.Composition = _defaultComposition;

            await UniTask.Yield();

            timeElapsed += Time.deltaTime;
        }

        _currentYScreenPosition = targetYScreenPosition;
        _defaultComposition.ScreenPosition = new(0f, _currentYScreenPosition);
        _composer.Composition = _defaultComposition;

    }


    //private async UniTask AdjustLensSettingsBasedOnVelocity(CancellationToken token)
    //{
    //    float previousClampedVelocity = Mathf.FloorToInt(_minVelocityThreshold / _changeThreshold) * _changeThreshold;

    //    while (!token.IsCancellationRequested)
    //    {
    //        float velocityY = Mathf.Abs(_playerRb.velocity.y);

    //        // Ограничиваем скорость по Y между порогами
    //        float clampedVelocity = Mathf.Clamp(velocityY, _minVelocityThreshold, _maxVelocityThreshold);

    //        // Округляем к ближайшему шагу (_changeThreshold)
    //        float roundedVelocity = Mathf.FloorToInt(clampedVelocity / _changeThreshold) * _changeThreshold;

    //        if (roundedVelocity != previousClampedVelocity)
    //        {
    //            // Нормализуем скорость для получения значения от 0 до 1
    //            float t = Mathf.InverseLerp(_minVelocityThreshold, _maxVelocityThreshold, roundedVelocity);

    //            // Применяем кривую интерполяции
    //            float interpolatedValue = _interpolationCurve.Evaluate(t);

    //            await SmoothlyAdjustLensSettings(interpolatedValue, token);

    //            previousClampedVelocity = roundedVelocity;
    //        }

    //        await UniTask.WaitForSeconds(_updateFrequency, cancellationToken: token);
    //    }
    //}

    //private async UniTask SmoothlyAdjustLensSettings(float interpolationFactor, CancellationToken token)
    //{
    //    float targetYPosition = Mathf.Lerp(_minYScreenPosition, _maxYScreenPosition, interpolationFactor);

    //    float startValue = _currentYScreenPosition;

    //    float duration = 1f;
    //    float timeElapsed = 0f;

    //    while (timeElapsed < duration && !token.IsCancellationRequested)
    //    {
    //        float t = timeElapsed / duration;
    //        float newValue = Mathf.Lerp(startValue, targetYPosition, t);

    //        _defaultComposition.ScreenPosition = new(0f, newValue);
    //        _composer.Composition = _defaultComposition;

    //        await UniTask.Yield(token);

    //        timeElapsed += Time.deltaTime;
    //    }

    //    _currentYScreenPosition = targetYPosition;

    //    _defaultComposition.ScreenPosition = new(0f, targetYPosition);
    //    _composer.Composition = _defaultComposition;
    //}

}
