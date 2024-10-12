using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class CharacterPositionMeter : MonoBehaviour
{
    public const int DRUM_NUMBER_OF_NUMBERS = 10;
    [Tooltip("Each index is a number, and in the value the y-axis position")]
    [SerializeField] float[] _drumTargetYPosition = new float[11];
    [SerializeField] RectTransform[] _drumTransform;
    [SerializeField] float _spareDrumNonVisiblePosition;
    [SerializeField] int value;

    int _drumCount;
    NativeArray<float> _targetYPositions;

    NativeArray<float> _textYPositions;
    NativeArray<float> _textSpeeds;
    NativeArray<bool> _useSpareDrum;
    NativeArray<int> _currentDigits;
    NativeArray<int> _targetDigits;

    Transform _playerTransform;
    JobHandle _counterHandle;

    public static float YPosition { get; private set; }
    public static float XPosition { get; private set; }

    [Inject]
    private void Construct(CharacterController player)
    {
        _playerTransform = player.transform;
    }

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
        _textSpeeds = new NativeArray<float>(_drumCount, Allocator.Persistent);
        _useSpareDrum = new NativeArray<bool>(_drumCount, Allocator.Persistent);
        _currentDigits = new NativeArray<int>(_drumCount, Allocator.Persistent);
        _targetDigits = new NativeArray<int>(_drumCount, Allocator.Persistent);
    }

    private async UniTask UpdatePlayerPosition()
    {
        var position = _playerTransform.position;

        var counter = new MechanicalCounterJob(ref _targetYPositions,
                                               ref _textYPositions,
                                               ref _textSpeeds,
                                               ref _currentDigits,
                                               ref _targetDigits
                                               );
        _counterHandle = counter.Schedule();
        _counterHandle.Complete();

        var deltaTime = Time.deltaTime;

        while (true)
        {
            position = _playerTransform.position;

            YPosition = position.y;
            XPosition = position.x;

            deltaTime = Time.deltaTime;

            counter.DeltaTime = deltaTime;
            counter.Value = (int)YPosition;

            _counterHandle = counter.Schedule();
            _counterHandle.Complete();

            for (int i = 0; i < _drumTransform.Length; i++)
            {
                SetDrumPosition(_drumTransform[i], i, _textYPositions[i]);
            }

            await UniTask.WaitForSeconds(deltaTime);
        }
    }

    private void SetDrumPosition(RectTransform drumTransform, int index, float drumYPosition)
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
        _textSpeeds.Dispose();
        _useSpareDrum.Dispose();
        _currentDigits.Dispose();
        _targetDigits.Dispose();
    }

}