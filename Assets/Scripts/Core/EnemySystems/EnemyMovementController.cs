using System;
using System.Threading;
using Additional;
using Cysharp.Threading.Tasks;
using Types.Structs.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Zenject;

namespace Core.Enemy
{
    // ReSharper disable once ClassNeverInstantiated.Global
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

        CancellationTokenSource _movementCts;

        event Action DisposeEvents;

        [Inject]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
        private void Construct(GlobalEventsManager globalEventsManager)
        {
            globalEventsManager.SubscribeToRoundStarted(RoundStart);
            globalEventsManager.SubscribeToRoundEnded(RoundEnd);

            DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
            DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);
        }

        private void RoundStart()
        {
            ClearToken(ref _movementCts);
            _movementCts = new();

            EnemyMoving(_movementCts.Token).Forget();
            EnemyInitialPlacement(_movementCts.Token).Forget();
        }

        private void RoundEnd()
        {
            ClearToken(ref _movementCts);
        }

        public void Initialize(int enemyCount,
                               Transform[] enemyTransforms,
                               float[] speeds,
                               EnumMotionPattern[] motionPatterns,
                               float2[] motionCharacteristic,
                               Vector2[] isolationDistance)
        {
            _enemyCount = enemyCount;

            CreateArrays(enemyTransforms, speeds, motionPatterns, motionCharacteristic, isolationDistance);
        }

        private void CreateArrays(Transform[] transforms,
                                  float[] speeds,
                                  EnumMotionPattern[] motionPatterns,
                                  float2[] motionCharacteristic,
                                  Vector2[] isolationDistance)
        {
            if (_enemyCount == transforms.Length - 1 &&
                _enemyCount == speeds.Length - 1 &&
                _enemyCount == motionPatterns.Length - 1 &&
                _enemyCount == motionCharacteristic.Length - 1 &&
                _enemyCount == isolationDistance.Length - 1)
            {
                Debug.LogError($"Not matching enemy counts " +
                               $"enemyCount = {_enemyCount}" +
                               $"transformsCount = {transforms.Length - 1}" +
                               $"speedsCount = {speeds.Length - 1}" +
                               $"motionPatternsCount = {motionPatterns.Length - 1}" +
                               $"motionCharacteristicCount = {motionCharacteristic.Length - 1}" +
                               $"isolationDistanceCount = {isolationDistance.Length - 1}");
                return;
            }

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

        private async UniTask EnemyInitialPlacement(CancellationToken token)
        {
            var initialPlacementJob = new InitialPlacementJob(
                positionProcessingMethods: ref _positionProcessingMethods,
                currentPosition: ref _currentEnemyPosition,
                targetPosition: ref _initialPosition,
                needToChange: ref _needToChangeInitialPosition,
                randoms: ref _randoms
                );

            var batchSize = 1;

            initialPlacementJob.PlayerYPosition = PlayerPositionMeter.YPosition;
            initialPlacementJob.PlayerXPosition = PlayerPositionMeter.XPosition;

            _enemyInitialPlacement = initialPlacementJob.Schedule(_enemyCount, batchSize, _enemyMoverHandler);

            var delaySpan = TimeSpan.FromSeconds(_initialPositionCheckDaley);

            while (true)
            {
                _enemyInitialPlacement.Complete();

                initialPlacementJob.PlayerYPosition = PlayerPositionMeter.YPosition;
                initialPlacementJob.PlayerXPosition = PlayerPositionMeter.XPosition;

                _enemyInitialPlacement = initialPlacementJob.Schedule(_enemyCount, batchSize, _enemyMoverHandler);

                await UniTask.Delay(delaySpan, cancellationToken: token);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private async UniTask EnemyMoving(CancellationToken token)
        {
            var enemyMoverJobs = new EnemyMoverJob(
                speeds: ref _speeds,
                initialPosition: ref _initialPosition,
                currentPosition: ref _currentEnemyPosition,
                motionCharacteristic: ref _motionCharacteristic,
                positionProcessingMethods: ref _positionProcessingMethods,
                needToChangeInitialPosition: ref _needToChangeInitialPosition
                )
            {
                Time = Time.time
            };

            _enemyMoverHandler = enemyMoverJobs.Schedule(_enemyTransforms, _enemyInitialPlacement);

            while (true)
            {
                _enemyMoverHandler.Complete();

                try
                {
                    enemyMoverJobs.Time = Time.time;
                    _enemyMoverHandler = enemyMoverJobs.Schedule(_enemyTransforms, _enemyInitialPlacement);
                }
                catch (ObjectDisposedException ex)
                {
                    Debug.Log($"ObjectDisposedException caught: {ex.Message}\n{ex.StackTrace}");
                    return;
                }

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken: token);
            }
        }

        public void UpdateEnemyValues(
            int index,
            float speed,
            EnumMotionPattern motionPattern,
            float2 motionCharacteristic,
            Vector2 spawnIsolateDistance)
        {
            if (_enemyCount - 1 < index) return;

            _enemyMoverHandler.Complete();
            _enemyInitialPlacement.Complete();

            _speeds[index] = speed;
            _positionProcessingMethods[index] = motionPattern;
            _motionCharacteristic[index] = motionCharacteristic;
            _isolateStaticElementDistance[index] = spawnIsolateDistance;
        }

        private void ClearToken(ref CancellationTokenSource cts) => ClearTokenSupport.ClearToken(ref cts);

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

            DisposeEvents?.Invoke();
        }

        public readonly struct Initializer
        {
            public readonly Transform[] Transforms;
            public readonly float[] Speeds;
            public readonly EnumMotionPattern[] EnumMotionPatterns;
            public readonly float2[] MotionCharacteristic;
            public readonly Vector2[] IsolationDistance;

            public Initializer(
                Transform[] transforms,
                float[] speeds,
                EnumMotionPattern[] enumMotionPatterns,
                float2[] motionCharacteristic,
                Vector2[] isolationDistance) : this()
            {
                Transforms = transforms;
                Speeds = speeds;
                EnumMotionPatterns = enumMotionPatterns;
                MotionCharacteristic = motionCharacteristic;
                IsolationDistance = isolationDistance;
            }
        }
    }
}