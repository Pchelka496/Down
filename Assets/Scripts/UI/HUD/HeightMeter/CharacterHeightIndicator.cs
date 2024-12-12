using Additional;
using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterHeightIndicator : MonoBehaviour
{
    public const int DRUM_NUMBER_OF_NUMBERS = 10;

    [Tooltip("Each index is a number, and in the value the y-axis position")]
    [SerializeField] float[] _drumTargetYPosition = new float[11];
    [SerializeField] RectTransform[] _drumTransform;
    [SerializeField] float _spareDrumNonVisiblePosition;
    [FormerlySerializedAs("value")][SerializeField] int _value;

    int _drumCount;
    NativeArray<float> _targetYPositions;

    NativeArray<float> _textYPositions;
    NativeArray<bool> _useSpareDrum;
    NativeArray<int> _currentDigits;
    NativeArray<int> _targetDigits;

    JobHandle _counterHandle;
    CancellationTokenSource _cts;

    System.Action DisposeEvents;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(GlobalEventsManager globalEventsManager)
    {
        globalEventsManager.SubscribeToRoundStarted(RoundStart);
        globalEventsManager.SubscribeToWarpStarted(WarpStart);
        globalEventsManager.SubscribeToRoundEnded(RoundEnd);

        DisposeEvents += () => globalEventsManager?.UnsubscribeFromWarpStarted(WarpStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);
    }

    private void Start()
    {
        _drumCount = _drumTransform.Length;
        CreateNativeArray();
        RoundEnd();

        ClearToken();
        _cts = new();

        UpdatePlayerPosition(_cts.Token).Forget();
    }

    private void RoundStart() => gameObject.SetActive(true);
    private void WarpStart() => gameObject.SetActive(true);
    private void RoundEnd() => gameObject.SetActive(false);

    private void CreateNativeArray()
    {
        if (_drumTargetYPosition.Length != DRUM_NUMBER_OF_NUMBERS)
        {
            Debug.LogError("_drumTargetYPosition.Length != DRUM_NUMBER_OF_NUMBERS");
            return;
        }

        _targetYPositions = new NativeArray<float>(DRUM_NUMBER_OF_NUMBERS, Allocator.Persistent);

        for (int i = 0; i < DRUM_NUMBER_OF_NUMBERS; i++)
        {
            _targetYPositions[i] = _drumTargetYPosition[i];
        }

        _textYPositions = new NativeArray<float>(_drumCount, Allocator.Persistent);
        _useSpareDrum = new NativeArray<bool>(_drumCount, Allocator.Persistent);
        _currentDigits = new NativeArray<int>(_drumCount, Allocator.Persistent);
        _targetDigits = new NativeArray<int>(_drumCount, Allocator.Persistent);
    }

    private async UniTaskVoid UpdatePlayerPosition(CancellationToken token)
    {
        var counter = new MechanicalCounterJob(ref _targetYPositions,
                                               ref _textYPositions,
                                               ref _currentDigits,
                                               ref _targetDigits
                                               )
        {
            Value = (int)CharacterPositionMeter.YPosition
        };

        _counterHandle = counter.Schedule();
        _counterHandle.Complete();

        while (true)
        {
            counter.Value = (int)CharacterPositionMeter.YPosition;

            _counterHandle = counter.Schedule();
            _counterHandle.Complete();

            for (int i = 0; i < _drumTransform.Length; i++)
            {
                SetDrumPosition(_drumTransform[i], _textYPositions[i]);
            }

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
        }
    }

    private void SetDrumPosition(RectTransform drumTransform, float drumYPosition)
    {
        drumTransform.localPosition = new(drumTransform.localPosition.x, drumYPosition);
    }

    private void ClearToken() => ClearTokenSupport.ClearToken(ref _cts);

    private void OnDestroy()
    {
        ClearToken();

        if (_counterHandle.IsCompleted == false)
        {
            _counterHandle.Complete();
        }

        _targetYPositions.Dispose();
        _textYPositions.Dispose();
        _useSpareDrum.Dispose();
        _currentDigits.Dispose();
        _targetDigits.Dispose();

        DisposeEvents?.Invoke();
    }
}
