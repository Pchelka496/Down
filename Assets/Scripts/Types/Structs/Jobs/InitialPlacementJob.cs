using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Types.Structs.Jobs
{
    [BurstCompile]
    public struct InitialPlacementJob : IJobParallelFor
    {
        const float STRONG_UPDATE_POSITION_Y_DISTANCE_TO_PLAYER = MAX_Y_TRAVEL_DISTANCE_TO_PLAYER + 50f;
        const float STRONG_UPDATE_POSITION_X_DISTANCE_TO_PLAYER = MAX_X_MOVABLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER + 20f;

        const float WEAK_UPDATE_POSITION_X_DISTANCE = 10f;
        const float WEAK_UPDATE_POSITION_Y_DISTANCE = 20f;

        const float MAX_X_STATIC_ENEMY_DISTANCE_TO_PLAYER = 30f;
        const float MAX_X_MOVABLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER = 120f;
        const float MIN_X_MOVABLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER = 30f;

        const float MAX_Y_TRAVEL_DISTANCE_TO_PLAYER = 300f;
        const float MIN_Y_TRAVEL_DISTANCE_TO_PLAYER = 120f;

        const float SAFE_ZONE_X_DISTANCE_TO_PLAYER = 50f;
        const float SAFE_ZONE_Y_DISTANCE_TO_PLAYER = 100f;

        const float SAFA_ZONE_X_RONDOM_OFFSET = 10f;
        const float SAFA_ZONE_Y_RONDOM_OFFSET = 20f;

        [ReadOnly] readonly NativeArray<EnumMotionPattern> _positionProcessingMethods;
        [WriteOnly] NativeArray<bool> _needToChange;

        [ReadOnly] NativeArray<Vector2> _currentPosition;
        [WriteOnly] NativeArray<Vector2> _targetPosition;

        public float PlayerYPosition;
        public float PlayerXPosition;

        NativeArray<Unity.Mathematics.Random> _randoms;

        public InitialPlacementJob(ref NativeArray<EnumMotionPattern> positionProcessingMethods,
                                   ref NativeArray<Vector2> currentPosition,
                                   ref NativeArray<Vector2> targetPosition,
                                   ref NativeArray<bool> needToChange,
                                   ref NativeArray<Unity.Mathematics.Random> randoms) : this()
        {
            _positionProcessingMethods = positionProcessingMethods;
            _currentPosition = currentPosition;
            _targetPosition = targetPosition;
            _needToChange = needToChange;
            _randoms = randoms;
        }

        public void Execute(int index)
        {
            var needsStrongXUpdate = math.abs(PlayerXPosition - _currentPosition[index].x) > STRONG_UPDATE_POSITION_X_DISTANCE_TO_PLAYER;
            var needsStrongYUpdate = math.abs(PlayerYPosition - _currentPosition[index].y) > STRONG_UPDATE_POSITION_Y_DISTANCE_TO_PLAYER;

            var random = _randoms[index];
            var newXPosition = _currentPosition[index].x;
            var newYPosition = _currentPosition[index].y;

            if (needsStrongXUpdate & needsStrongYUpdate)
            {
                newXPosition = CalculateStrongX(index, ref random);
                newYPosition = CalculateStrongY(ref random);
            }
            else if (needsStrongXUpdate)
            {
                newXPosition = CalculateStrongX(index, ref random);
                newYPosition = CalculateWeakY(index, ref random);
            }
            else if (needsStrongYUpdate)
            {
                newYPosition = CalculateStrongY(ref random);
                newXPosition = CalculateWeakX(index, ref random);
            }

            if (needsStrongXUpdate || needsStrongYUpdate)
            {
                _targetPosition[index] = AdjustPositionIfTooClose(new(newXPosition, newYPosition), ref random);
                _needToChange[index] = true;
            }

            _randoms[index] = random;
        }

        private readonly float CalculateStrongX(int index, ref Unity.Mathematics.Random random)
        {
            if (_positionProcessingMethods[index] == EnumMotionPattern.LinearHorizontalRight ||
                _positionProcessingMethods[index] == EnumMotionPattern.WavyRight ||
                _positionProcessingMethods[index] == EnumMotionPattern.JerkyRight)
            {
                return PlayerXPosition - random.NextFloat(MIN_X_MOVABLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER, MAX_X_MOVABLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER);
            }
            else if (_positionProcessingMethods[index] == EnumMotionPattern.LinearHorizontalLeft ||
                     _positionProcessingMethods[index] == EnumMotionPattern.WavyLeft ||
                     _positionProcessingMethods[index] == EnumMotionPattern.JerkyLeft)
            {
                return PlayerXPosition + random.NextFloat(MIN_X_MOVABLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER, MAX_X_MOVABLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER);
            }
            else
            {
                return PlayerXPosition + random.NextFloat(-MAX_X_STATIC_ENEMY_DISTANCE_TO_PLAYER, MAX_X_STATIC_ENEMY_DISTANCE_TO_PLAYER);
            }
        }

        private readonly float CalculateStrongY(ref Unity.Mathematics.Random random)
        {
            return PlayerYPosition - random.NextFloat(MIN_Y_TRAVEL_DISTANCE_TO_PLAYER, MAX_Y_TRAVEL_DISTANCE_TO_PLAYER);
        }

        private readonly float CalculateWeakX(int index, ref Unity.Mathematics.Random random)
        {
            return _currentPosition[index].x + random.NextFloat(-WEAK_UPDATE_POSITION_X_DISTANCE, WEAK_UPDATE_POSITION_X_DISTANCE);
        }

        private readonly float CalculateWeakY(int index, ref Unity.Mathematics.Random random)
        {
            return _currentPosition[index].y + random.NextFloat(-WEAK_UPDATE_POSITION_Y_DISTANCE, WEAK_UPDATE_POSITION_Y_DISTANCE);
        }

        private readonly Vector2 AdjustPositionIfTooClose(Vector2 currentTargetPosition, ref Unity.Mathematics.Random random)
        {
            if (math.abs(PlayerYPosition - currentTargetPosition.y) < SAFE_ZONE_Y_DISTANCE_TO_PLAYER)
            {
                if (currentTargetPosition.y < PlayerYPosition)
                {
                    currentTargetPosition.y = PlayerYPosition - (SAFE_ZONE_Y_DISTANCE_TO_PLAYER + random.NextFloat(SAFA_ZONE_Y_RONDOM_OFFSET));
                }
                else
                {
                    currentTargetPosition.y = PlayerYPosition + (SAFE_ZONE_Y_DISTANCE_TO_PLAYER + random.NextFloat(SAFA_ZONE_Y_RONDOM_OFFSET));
                }

                return currentTargetPosition;
            }

            if (math.abs(PlayerXPosition - currentTargetPosition.x) < SAFE_ZONE_X_DISTANCE_TO_PLAYER && PlayerYPosition < currentTargetPosition.y)
            {
                if (currentTargetPosition.x < PlayerXPosition)
                {
                    currentTargetPosition.x = PlayerXPosition - (SAFE_ZONE_X_DISTANCE_TO_PLAYER + random.NextFloat(SAFA_ZONE_X_RONDOM_OFFSET));
                }
                else
                {
                    currentTargetPosition.x = PlayerXPosition + (SAFE_ZONE_X_DISTANCE_TO_PLAYER + random.NextFloat(SAFA_ZONE_X_RONDOM_OFFSET));
                }
            }

            return currentTargetPosition;
        }
    }
}
