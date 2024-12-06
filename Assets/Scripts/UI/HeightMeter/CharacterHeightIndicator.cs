using Cysharp.Threading.Tasks;
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
    [FormerlySerializedAs("value")] [SerializeField] int _value;

    int _drumCount;
    NativeArray<float> _targetYPositions;

    NativeArray<float> _textYPositions;
    NativeArray<bool> _useSpareDrum;
    NativeArray<int> _currentDigits;
    NativeArray<int> _targetDigits;

    JobHandle _counterHandle;

    private void Start()
    {
        _drumCount = _drumTransform.Length;
        CreateNativeArray();
        UpdatePlayerPosition().Forget();
    }

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

    private async UniTaskVoid UpdatePlayerPosition()
    {
        var counter = new MechanicalCounterJob(ref _targetYPositions,
                                               ref _textYPositions,
                                               ref _currentDigits,
                                               ref _targetDigits
                                               )
        {
            DeltaTime = Time.deltaTime,
            Value = (int)CharacterPositionMeter.YPosition
        };

        _counterHandle = counter.Schedule();
        _counterHandle.Complete();

        while (true)
        {
            var deltaTime = Time.deltaTime;

            counter.DeltaTime = deltaTime;
            counter.Value = (int)CharacterPositionMeter.YPosition;

            _counterHandle = counter.Schedule();
            _counterHandle.Complete();

            for (int i = 0; i < _drumTransform.Length; i++)
            {
                SetDrumPosition(_drumTransform[i], _textYPositions[i]);
            }

            await UniTask.WaitForSeconds(deltaTime);
        }
    }

    private void SetDrumPosition(RectTransform drumTransform, float drumYPosition)
    {
        drumTransform.localPosition = new(drumTransform.localPosition.x, drumYPosition);
    }

    private void OnDestroy()
    {
        if (_counterHandle.IsCompleted == false)
        {
            _counterHandle.Complete();
        }

        _targetYPositions.Dispose();
        _textYPositions.Dispose();
        _useSpareDrum.Dispose();
        _currentDigits.Dispose();
        _targetDigits.Dispose();
    }

}