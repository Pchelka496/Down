using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Unity.Cinemachine;
using Unity.Collections.LowLevel.Unsafe;


[System.Serializable]
public class PositionComposerController
{
    [SerializeField] ScreenComposerSettings _defaultComposition;
    [SerializeField] CinemachinePositionComposer _composer;
    [SerializeField] float _maxYScreenPosition = 10f;
    [SerializeField] float _minYScreenPosition = 5f;
    [SerializeField] float _transitionSpeed = 5f;

    [SerializeField] float _minVelocityThreshold = 0f;
    [SerializeField] float _maxVelocityThreshold = 20f;

    [SerializeField] float _updateFrequency = 1f;

    float _currentYScreenPosition;

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
            await UniTask.Yield(PlayerLoopTiming.Update);
        }
    }

    private async UniTask AdjustLensSettingsBasedOnVelocity()
    {
        // �������� ������������ �������� ������
        float velocityY = _playerRb.velocity.y;

        // ����������� �������� � ��������� �� 0 �� 1
        float normalizedVelocity = Mathf.InverseLerp(_minVelocityThreshold, _maxVelocityThreshold, velocityY);
        normalizedVelocity = Mathf.Clamp01(normalizedVelocity); // ������������ �������� �� 0 �� 1

        // ��������� ����� �������� YScreenPosition �� ������ ��������������� ��������
        float targetYScreenPosition = Mathf.Lerp(_minYScreenPosition, _maxYScreenPosition, normalizedVelocity);

        // ������ ������������� ������� �������� YScreenPosition � �������� ��������
        _currentYScreenPosition = Mathf.Lerp(_currentYScreenPosition, targetYScreenPosition, Time.deltaTime * _transitionSpeed);

        // ������������� ����� �������� YScreenPosition � composer
        _defaultComposition.ScreenPosition = new(0f, _currentYScreenPosition);
        _composer.Composition = _defaultComposition;

        await UniTask.Yield();
    }

    //private async UniTask AdjustLensSettingsBasedOnVelocity(CancellationToken token)
    //{
    //    float previousClampedVelocity = Mathf.FloorToInt(_minVelocityThreshold / _changeThreshold) * _changeThreshold;

    //    while (!token.IsCancellationRequested)
    //    {
    //        float velocityY = Mathf.Abs(_playerRb.velocity.y);

    //        // ������������ �������� �� Y ����� ��������
    //        float clampedVelocity = Mathf.Clamp(velocityY, _minVelocityThreshold, _maxVelocityThreshold);

    //        // ��������� � ���������� ���� (_changeThreshold)
    //        float roundedVelocity = Mathf.FloorToInt(clampedVelocity / _changeThreshold) * _changeThreshold;

    //        if (roundedVelocity != previousClampedVelocity)
    //        {
    //            // ����������� �������� ��� ��������� �������� �� 0 �� 1
    //            float t = Mathf.InverseLerp(_minVelocityThreshold, _maxVelocityThreshold, roundedVelocity);

    //            // ��������� ������ ������������
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
