using System;
using System.Threading;
using Additional;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class CustomPressButton : Button, IPointerDownHandler, IPointerUpHandler
{
    [Header("Functionality")]
    [Tooltip("If true, button clamping processing is disabled.")]
    [SerializeField]
    bool _isProcessingDisabled = true;

    [Header("Check Settings")]
    [Tooltip("Minimum delay between checks(in seconds).")]
    [NaughtyAttributes.HideIf("_isProcessingDisabled")]
    [SerializeField]
    float _minCheckDelay = 0.1f;

    [Tooltip("Maximum delay between checks(in seconds).")]
    [NaughtyAttributes.HideIf("_isProcessingDisabled")]
    [SerializeField]
    float _maxCheckDelay = 0.4f;

    [Tooltip("Interpolation Factor(0 to 1).")]
    [NaughtyAttributes.HideIf("_isProcessingDisabled")]
    [SerializeField]
    [Range(0f, 1f)]
    float _lerpFactor = 0.2f;

    bool _isButtonPressed;
    CancellationTokenSource _cts;

    public bool IsProcessingDisabled { get => _isProcessingDisabled; set => _isProcessingDisabled = value; }

    public override void OnPointerDown(PointerEventData eventData)
    {
        _isButtonPressed = true;
        base.OnPointerDown(eventData);

        if (_isProcessingDisabled)
            return;

        ClearToken();
        _cts = new();
        StartCheckingState(_cts.Token).Forget();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        _isButtonPressed = false;
        base.OnPointerUp(eventData);

        ClearToken();
    }

    private async UniTaskVoid StartCheckingState(CancellationToken token)
    {
        try
        {
            var currentDelay = _minCheckDelay;
            while (!token.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(currentDelay), cancellationToken: token);
                currentDelay = Mathf.Lerp(currentDelay, _maxCheckDelay, _lerpFactor);

                if (_isButtonPressed)
                {
                    onClick?.Invoke();
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void ClearToken() => ClearTokenSupport.ClearToken(ref _cts);

    protected override void OnDestroy()
    {
        base.OnDestroy();
        ClearToken();
    }
}

