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
        const float UPDATE_POSITION_Y_DISTANCE_TO_PLAYER = 250f;
        const float UPDATE_POSITION_X_DISTANCE_TO_PLAYER = 60f;

        const float MAX_X_STATIC_ENEMY_DISTANCE_TO_PLAYER = 15f;
        const float MAX_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER = 50f;
        const float MIN_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER = 0f;

        const float MAX_Y_TRAVEL_DISTANCE_TO_PLAYER = 300f;
        const float MIN_Y_TRAVEL_DISTANCE_TO_PLAYER = 120f;

        [ReadOnly] readonly NativeArray<EnumMotionPattern> _positionProcessingMethods;
        [WriteOnly] NativeArray<bool> _needToChange;

        [ReadOnly] private NativeArray<Vector2> _currentPosition;
        private NativeArray<Vector2> _targetPosition;

        public float PlayerYPosition;
        public float PlayerXPosition;

        NativeArray<Unity.Mathematics.Random> _randoms;

        public InitialPlacementJob(ref NativeArray<EnumMotionPattern> positionProcessingMethods,
            ref NativeArray<Vector2> currentPosition,
            ref NativeArray<Vector2> targetPosition,
            ref NativeArray<bool> needToChange,
            ref NativeArray<Unity.Mathematics.Random> randoms
        ) : this()
        {
            _positionProcessingMethods = positionProcessingMethods;
            _currentPosition = currentPosition;
            _targetPosition = targetPosition;
            _needToChange = needToChange;
            _randoms = randoms;
        }

        public void Execute(int index)
        {
            if (math.abs(PlayerYPosition - _currentPosition[index].y) > UPDATE_POSITION_Y_DISTANCE_TO_PLAYER
                || math.abs(PlayerXPosition - _currentPosition[index].x) > UPDATE_POSITION_X_DISTANCE_TO_PLAYER)
            {
                var random = _randoms[index];
                var newXPosition = GlobalXCalculation(index, ref random);
                var newYPosition = GlobalYCalculation(ref random);

                _targetPosition[index] = new Vector2(newXPosition, newYPosition);
                _needToChange[index] = true;

                _randoms[index] = random;
            }
        }

        private readonly float GlobalXCalculation(int index, ref Unity.Mathematics.Random random)
        {
            if (_positionProcessingMethods[index] == EnumMotionPattern.LinearHorizontalRight ||
                _positionProcessingMethods[index] == EnumMotionPattern.WavyRight ||
                _positionProcessingMethods[index] == EnumMotionPattern.JerkyRight)
            {
                return PlayerXPosition - random.NextFloat(MIN_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER,
                    MAX_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER);
            }
            else if (_positionProcessingMethods[index] == EnumMotionPattern.LinearHorizontalLeft ||
                     _positionProcessingMethods[index] == EnumMotionPattern.WavyLeft ||
                     _positionProcessingMethods[index] == EnumMotionPattern.JerkyLeft)
            {
                return PlayerXPosition + random.NextFloat(MIN_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER,
                    MAX_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER);
            }
            else
            {
                return PlayerXPosition + random.NextFloat(-MAX_X_STATIC_ENEMY_DISTANCE_TO_PLAYER,
                    MAX_X_STATIC_ENEMY_DISTANCE_TO_PLAYER);
            }
        }

        private readonly float GlobalYCalculation(ref Unity.Mathematics.Random random)
        {
            return PlayerYPosition - random.NextFloat(MIN_Y_TRAVEL_DISTANCE_TO_PLAYER, MAX_Y_TRAVEL_DISTANCE_TO_PLAYER);
        }
    }
}