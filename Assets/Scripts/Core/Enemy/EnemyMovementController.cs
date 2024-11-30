using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using System;
using Unity.Jobs;

public class EnemyMovementController : IDisposable
{
    readonly float _initialPositionCheckDaley = 0.5f;
    int _enemyCount = 50;
    JobHandle _enemyMoverHandler;
    JobHandle _enemyInitialPlacement;

    NativeArray<EnumMotionPattern> _positionProcessingMethods;
    NativeArray<float2> _motionCharacteristic;
    NativeArray<Vector2> _currentEnemyPosition;
    NativeArray<Vector2> _isolateStaticElementDistance;
    NativeArray<float> _speeds;
    NativeArray<bool> _needToChangeInitialPosition;
    NativeArray<Vector2> _initialPosition;
    NativeArray<Unity.Mathematics.Random> _randoms;
    TransformAccessArray _enemyTransforms;

    public void Initialize(int enemyCount,
                           Transform[] enemyTransforms,
                           float[] speeds,
                           EnumMotionPattern[] motionPatterns,
                           float2[] motionCharacteristic,
                           Vector2[] isolationDistance
                           )
    {
        _enemyCount = enemyCount;

        CreateArrays(enemyTransforms, speeds, motionPatterns, motionCharacteristic, isolationDistance);
    }

    public void StartEnemyMoving()
    {
        EnemyMoving().Forget();
        EnemyInitialPlacement().Forget();
    }

    private void CreateArrays(Transform[] transforms, float[] speeds, EnumMotionPattern[] motionPatterns, float2[] motionCharacteristic, Vector2[] isolationDistance)
    {
        if (_enemyCount == transforms.Length - 1 && _enemyCount == speeds.Length - 1 && _enemyCount == motionPatterns.Length - 1) return;

        _speeds = new(speeds, Allocator.Persistent);
        _isolateStaticElementDistance = new(isolationDistance, Allocator.Persistent);
        _positionProcessingMethods = new(motionPatterns, Allocator.Persistent);
        _motionCharacteristic = new(motionCharacteristic, Allocator.Persistent);
        _currentEnemyPosition = new(_enemyCount, Allocator.Persistent);
        _initialPosition = new(_enemyCount, Allocator.Persistent);
        _needToChangeInitialPosition = new(_enemyCount, Allocator.Persistent);
        _enemyTransforms = new(transforms);

        _randoms = new(_enemyCount, Allocator.Persistent);

        FillRandomsArray();
    }

    private void FillRandomsArray()
    {
        uint seed = (uint)UnityEngine.Random.Range(1, int.MaxValue);

        for (int i = 0; i < _enemyCount; i++)
        {
            _randoms[i] = new(seed + (uint)i);
        }
    }

    private async UniTask EnemyInitialPlacement()
    {
        var initialPlacementJob = new InitialPlacementJob(positionProcessingMethods: ref _positionProcessingMethods,
                                                          currentPosition: ref _currentEnemyPosition,
                                                          targetPosition: ref _initialPosition,
                                                          needToChange: ref _needToChangeInitialPosition,
                                                          randoms: ref _randoms
                                                          );

        var batchSize = 1;

        initialPlacementJob.PlayerYPosition = CharacterPositionMeter.YPosition;
        initialPlacementJob.PlayerXPosition = CharacterPositionMeter.XPosition;

        _enemyInitialPlacement = initialPlacementJob.Schedule(_enemyCount, batchSize, _enemyMoverHandler);

        var delaySpan = TimeSpan.FromSeconds(_initialPositionCheckDaley);

        while (true)
        {
            _enemyInitialPlacement.Complete();

            initialPlacementJob.PlayerYPosition = CharacterPositionMeter.YPosition;
            initialPlacementJob.PlayerXPosition = CharacterPositionMeter.XPosition;

            _enemyInitialPlacement = initialPlacementJob.Schedule(_enemyCount, batchSize, _enemyMoverHandler);

            await UniTask.Delay(delaySpan);
        }
    }

    private async UniTask EnemyMoving()
    {
        var enemyMoverJobs = new EnemyMoverJob(speeds: ref _speeds,
                                               initialPosition: ref _initialPosition,
                                               currentPosition: ref _currentEnemyPosition,
                                               motionCharacteristic: ref _motionCharacteristic,
                                               positionProcessingMethods: ref _positionProcessingMethods,
                                               needToChangeInitialPosition: ref _needToChangeInitialPosition
                                               )
        {
            DeltaTime = Time.deltaTime
        };

        _enemyMoverHandler = enemyMoverJobs.Schedule(_enemyTransforms, _enemyInitialPlacement);

        while (true)
        {
            enemyMoverJobs.DeltaTime = Time.deltaTime;

            _enemyMoverHandler.Complete();

            try
            {
                _enemyMoverHandler = enemyMoverJobs.Schedule(_enemyTransforms, _enemyInitialPlacement);
            }
            catch (ObjectDisposedException ex)
            {
                Debug.LogWarning($"ObjectDisposedException caught: {ex.Message}\n{ex.StackTrace}");
                return;
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
        }
    }

    public void UpdateEnemyValues(int index, float speed, EnumMotionPattern motionPattern, float2 motionCharacteristic, Vector2 spawnIsolateDistance)
    {
        if (_enemyCount - 1 < index) return;

        _enemyMoverHandler.Complete();
        _enemyInitialPlacement.Complete();

        _speeds[index] = speed;
        _positionProcessingMethods[index] = motionPattern;
        _motionCharacteristic[index] = motionCharacteristic;
        _isolateStaticElementDistance[index] = spawnIsolateDistance;
    }

    public void Dispose()
    {
        _enemyMoverHandler.Complete();
        _enemyMoverHandler = default;

        _enemyInitialPlacement.Complete();
        _enemyInitialPlacement = default;

        _speeds.Dispose();
        _positionProcessingMethods.Dispose();
        _motionCharacteristic.Dispose();
        _isolateStaticElementDistance.Dispose();
        _currentEnemyPosition.Dispose();
        _initialPosition.Dispose();
        _needToChangeInitialPosition.Dispose();
        _enemyTransforms.Dispose();
        _randoms.Dispose();
    }

}
