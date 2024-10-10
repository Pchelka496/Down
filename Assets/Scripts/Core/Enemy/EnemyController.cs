using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using Zenject;
using Unity.Mathematics;
using System;
using Unity.Jobs;

public class EnemyController : IDisposable
{
    int _enemyCount = 50;
    JobHandle _enemyMoverHandler;
    JobHandle _enemyInitialPlacement;

    NativeArray<float> _speeds;
    NativeArray<EnumMotionPattern> _positionProcessingMethods;
    NativeArray<float2> _motionCharacteristic;
    NativeArray<Vector2> _currentEnemyPosition;
    NativeArray<bool> _needToChangeInitialPosition;
    NativeArray<Vector2> _initialPosition;
    NativeArray<Vector2> _isolationDistance;
    TransformAccessArray _enemyTransforms;

    [Inject]
    private void Construct()
    {

    }

    public void Initialize(int enemyCount)
    {
        _enemyCount = enemyCount;
    }

    public void RoundStart(Transform[] enemyTransforms, float[] speeds, EnumMotionPattern[] motionPatterns, float2[] motionCharacteristic, Vector2[] isolationDistance)
    {
        CreateArrays(enemyTransforms, speeds, motionPatterns, motionCharacteristic, isolationDistance);

        EnemyMoving().Forget();
        EnemyInitialPlacement().Forget();
    }

    private void CreateArrays(Transform[] transforms, float[] speeds, EnumMotionPattern[] motionPatterns, float2[] motionCharacteristic, Vector2[] isolationDistance)
    {
        if (_enemyCount == transforms.Length - 1 && _enemyCount == speeds.Length - 1 && _enemyCount == motionPatterns.Length - 1) return;

        _speeds = new(speeds, Allocator.Persistent);
        _positionProcessingMethods = new(motionPatterns, Allocator.Persistent);
        _motionCharacteristic = new(motionCharacteristic, Allocator.Persistent);
        _currentEnemyPosition = new(_enemyCount, Allocator.Persistent);
        _initialPosition = new(_enemyCount, Allocator.Persistent);
        _needToChangeInitialPosition = new(_enemyCount, Allocator.Persistent);
        _isolationDistance = new(isolationDistance, Allocator.Persistent);
        _enemyTransforms = new(transforms);
    }

    private async UniTask EnemyInitialPlacement()
    {
        var initialPlacementJob = new InitialPlacementJob(ref _positionProcessingMethods,
                                                          ref _currentEnemyPosition,
                                                          ref _isolationDistance,
                                                          ref _initialPosition,
                                                          ref _needToChangeInitialPosition);

        var deltaTime = Time.deltaTime;

        var batchSize = _enemyCount / 4;

        // Указываем зависимость для InitialPlacementJob
        _enemyInitialPlacement = initialPlacementJob.Schedule(_enemyCount, batchSize, _enemyMoverHandler);

        while (true)
        {
            deltaTime = Time.deltaTime;

            _enemyInitialPlacement.Complete();  // Дождаться завершения предыдущей работы

            _enemyInitialPlacement = initialPlacementJob.Schedule(_enemyCount, batchSize, _enemyMoverHandler);  // Снова указываем зависимость

            await UniTask.WaitForSeconds(deltaTime);
        }
    }

    private async UniTask EnemyMoving()
    {
        var enemyMoverJobs = new EnemyMoverJob(ref _speeds,
                                                ref _positionProcessingMethods,
                                                ref _motionCharacteristic,
                                                ref _currentEnemyPosition,
                                                ref _initialPosition,
                                                ref _needToChangeInitialPosition);

        var deltaTime = Time.deltaTime;

        enemyMoverJobs.DeltaTime = deltaTime;

        // Указываем зависимость для EnemyMoverJobs
        _enemyMoverHandler = enemyMoverJobs.Schedule(_enemyTransforms, _enemyInitialPlacement);

        while (true)
        {
            deltaTime = Time.deltaTime;

            enemyMoverJobs.Time = deltaTime;
            enemyMoverJobs.DeltaTime = deltaTime;

            _enemyMoverHandler.Complete();  // Дождаться завершения предыдущей работы

            _enemyMoverHandler = enemyMoverJobs.Schedule(_enemyTransforms, _enemyInitialPlacement);  // Снова указываем зависимость

            await UniTask.WaitForSeconds(deltaTime);
        }
    }


    public void UpdateEnemyValues(int index, float speed, EnumMotionPattern motionPattern, float2 motionCharacteristic, Vector2 isolationDistance)
    {
        if (_enemyCount - 1 < index) return;

        if (_enemyInitialPlacement == default || _enemyMoverHandler == default) return;

        _enemyMoverHandler.Complete();
        _enemyInitialPlacement.Complete();

        _speeds[index] = speed;
        _positionProcessingMethods[index] = motionPattern;
        _motionCharacteristic[index] = motionCharacteristic;
        _isolationDistance[index] = isolationDistance;
    }

    public void Dispose()
    {
        if (_enemyMoverHandler != null)
        {
            _enemyMoverHandler.Complete();
            _enemyMoverHandler = default;
        }
        if (_enemyInitialPlacement != null)
        {
            _enemyInitialPlacement.Complete();
            _enemyInitialPlacement = default;
        }

        _speeds.Dispose();
        _positionProcessingMethods.Dispose();
        _motionCharacteristic.Dispose();
        _currentEnemyPosition.Dispose();
        _initialPosition.Dispose();
        _needToChangeInitialPosition.Dispose();
        _isolationDistance.Dispose();
        _enemyTransforms.Dispose();
    }

}

public readonly struct TransformValues
{
    public readonly Vector2 Position;
    public readonly Quaternion Rotation;

}