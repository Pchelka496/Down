using Unity.Cinemachine;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

[System.Serializable]
public class LensController
{
    const float _lensFarClipPlane = 1000f;
    const float _lensNearClipPlane = 0.3f;
    const bool _lensIsOrthographic = true;

    [SerializeField] float _maxOrthographicSize = 10f;
    [SerializeField] float _minOrthographicSize = 5f;

    [SerializeField] float _minVelocityThreshold = 10f;
    [SerializeField] float _maxVelocityThreshold = 40f;
    [SerializeField] float _changeThreshold = 5f;

    [SerializeField] AnimationCurve _lensInterpolationCurve;
    [SerializeField] float _updateFrequency = 1f;

    CinemachineCamera _camera;
    Rigidbody2D _player;
    CancellationTokenSource _cts;

    public void Initialize(CinemachineCamera camera, Rigidbody2D player)
    {
        _camera = camera;
        _player = player;
        StartAdjustingLensSettingsAsync().Forget();
    }

    private async UniTaskVoid StartAdjustingLensSettingsAsync()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        try
        {
            await AdjustLensSettingsBasedOnVelocity(token);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Lens adjustment was canceled.");
        }
    }

    private async UniTask AdjustLensSettingsBasedOnVelocity(CancellationToken token)
    {
        // ������� "���������" �������� ��������
        float previousClampedVelocity = Mathf.FloorToInt(_minVelocityThreshold / _changeThreshold) * _changeThreshold;

        while (!token.IsCancellationRequested)
        {
            float velocityY = Mathf.Abs(_player.velocity.y);

            // ������������ �������� �� Y ����� ��������
            float clampedVelocity = Mathf.Clamp(velocityY, _minVelocityThreshold, _maxVelocityThreshold);

            // ��������� � ���������� ���� (_changeThreshold)
            float roundedVelocity = Mathf.FloorToInt(clampedVelocity / _changeThreshold) * _changeThreshold;

            // ���������, ��������� �� ���
            if (roundedVelocity != previousClampedVelocity)
            {
                // ����������� �������� ��� ��������� �������� �� 0 �� 1
                float t = Mathf.InverseLerp(_minVelocityThreshold, _maxVelocityThreshold, roundedVelocity);

                // ��������� ������ ������������
                float interpolatedValue = _lensInterpolationCurve.Evaluate(t);

                await SmoothlyAdjustLensSettings(interpolatedValue, token);

                previousClampedVelocity = roundedVelocity;
            }

            await UniTask.WaitForSeconds(_updateFrequency, cancellationToken: token);
        }
    }


    private async UniTask SmoothlyAdjustLensSettings(float interpolationFactor, CancellationToken token)
    {
        // ������� �������� ���������������� �������
        float targetOrthographicSize = Mathf.Lerp(_minOrthographicSize, _maxOrthographicSize, interpolationFactor);

        // ��������� �������� ������� �����
        float startValue = _camera.Lens.OrthographicSize;

        float duration = 1f;
        float timeElapsed = 0f;

        while (timeElapsed < duration && !token.IsCancellationRequested)
        {
            // �������� ������������ �� �������
            float t = timeElapsed / duration;
            float newValue = Mathf.Lerp(startValue, targetOrthographicSize, t);

            // ������������� ����� �������� ��� ���������������� �������
            var lensSettings = _camera.Lens;
            lensSettings.OrthographicSize = newValue;
            lensSettings.FarClipPlane = _lensFarClipPlane;
            lensSettings.NearClipPlane = _lensNearClipPlane;

            _camera.Lens = lensSettings;

            // �������� ���������� �����
            await UniTask.Yield(token);

            // ��������� �����
            timeElapsed += Time.deltaTime;
        }

        var finalLensSettings = _camera.Lens;
        finalLensSettings.OrthographicSize = targetOrthographicSize;
        finalLensSettings.FarClipPlane = _lensFarClipPlane;
        finalLensSettings.NearClipPlane = _lensNearClipPlane;

        _camera.Lens = finalLensSettings;
    }

}
