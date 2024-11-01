using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using Zenject;
using Unity.Mathematics;
using System;
using Unity.Jobs;
using System.Collections.Generic;

public class EnemyMovementController : IDisposable
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
    TransformAccessArray _enemyTransforms;

    public void Initialize(int enemyCount, Transform[] enemyTransforms, float[] speeds, EnumMotionPattern[] motionPatterns, float2[] motionCharacteristic, Vector2[] isolationDistance)
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
        _positionProcessingMethods = new(motionPatterns, Allocator.Persistent);
        _motionCharacteristic = new(motionCharacteristic, Allocator.Persistent);
        _currentEnemyPosition = new(_enemyCount, Allocator.Persistent);
        _initialPosition = new(_enemyCount, Allocator.Persistent);
        _needToChangeInitialPosition = new(_enemyCount, Allocator.Persistent);
        _enemyTransforms = new(transforms);
    }

    private async UniTask EnemyInitialPlacement()
    {
        var initialPlacementJob = new InitialPlacementJob(ref _positionProcessingMethods,
                                                          ref _currentEnemyPosition,
                                                          ref _initialPosition,
                                                          ref _needToChangeInitialPosition);

        //var deltaTime = Time.deltaTime;
        var batchSize = _enemyCount / 4;

        _enemyInitialPlacement = initialPlacementJob.Schedule(_enemyCount, batchSize, _enemyMoverHandler);

        while (true)
        {
            //deltaTime = Time.deltaTime;

            _enemyInitialPlacement.Complete();

            _enemyInitialPlacement = initialPlacementJob.Schedule(_enemyCount, batchSize, _enemyMoverHandler);

            await UniTask.WaitForSeconds(0.5f);
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

        _enemyMoverHandler = enemyMoverJobs.Schedule(_enemyTransforms, _enemyInitialPlacement);

        while (true)
        {
            deltaTime = Time.deltaTime;

            enemyMoverJobs.Time = deltaTime;
            enemyMoverJobs.DeltaTime = deltaTime;

            _enemyMoverHandler.Complete();

            _enemyMoverHandler = enemyMoverJobs.Schedule(_enemyTransforms, _enemyInitialPlacement);

            await UniTask.WaitForSeconds(deltaTime);
        }
    }

    public void UpdateEnemyValues(int index, float speed, EnumMotionPattern motionPattern, float2 motionCharacteristic)
    {
        if (_enemyCount - 1 < index) return;

        _enemyMoverHandler.Complete();
        _enemyInitialPlacement.Complete();

        _speeds[index] = speed;
        _positionProcessingMethods[index] = motionPattern;
        _motionCharacteristic[index] = motionCharacteristic;
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
        _currentEnemyPosition.Dispose();
        _initialPosition.Dispose();
        _needToChangeInitialPosition.Dispose();
        _enemyTransforms.Dispose();
    }

}
