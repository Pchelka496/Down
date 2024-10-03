using Cysharp.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using Zenject;

public class FlightModule : IFlightModule
{
    const float SIDE_MOVE_GRAVITY_MULTIPLIER = 0.8f;
    const float MAX_INTROPALITES_INPUT_VALUE = 0.5f;
    const float MIN_INTROPALITES_INPUT_VALUE = 0.05f;

    float _maxNegativeGravity = -0.5f;
    float _maxLinearDrag = 5f;
    float _minLinearDrag = 0.2f;

    float _maxMoveSpeed = 0.1f;

    Controls _controls;
    float _defaultGravityScale = 1f;
    float _currentMoveSpeed = 0f;

    float _oldLeftInputValue;
    float _oldRightInputValue;
    float _oldLinearDrag;

    Rigidbody2D _rb;
    Transform _transform;

    [Inject]
    private void Construct(CharacterController character, Controls controls)
    {
        _rb = character.Rb;
        _transform = character.transform;
        _controls = controls;
    }

    public async UniTask Fly(CancellationTokenSource cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var newLeftInputValue = _controls.Player.MoveLeft.ReadValue<float>();
            var newRightInputValue = _controls.Player.MoveRight.ReadValue<float>();

            var intraplateValue = MIN_INTROPALITES_INPUT_VALUE;

            if (newLeftInputValue != 0f && newRightInputValue != 0f)
            {
                intraplateValue = MAX_INTROPALITES_INPUT_VALUE;
            }

            _oldLeftInputValue = Mathf.Lerp(_oldLeftInputValue, newLeftInputValue, intraplateValue);
            _oldRightInputValue = Mathf.Lerp(_oldRightInputValue, newRightInputValue, intraplateValue);

            if (_oldLeftInputValue != 1f || _oldRightInputValue != 1f)
            {
                XAxisManipulation(_oldLeftInputValue, _oldRightInputValue);
            }
            else
            {
                _currentMoveSpeed = 0f;
            }

            YAxisManipulation(_oldLeftInputValue, _oldRightInputValue);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void XAxisManipulation(float leftInput, float rightInput)
    {
        var moveSpeed = _maxMoveSpeed * (Mathf.Max(leftInput, rightInput) - Mathf.Min(leftInput, rightInput));
        if (leftInput > rightInput)
        {
            moveSpeed = -moveSpeed;
        }

        _currentMoveSpeed = Mathf.Lerp(_currentMoveSpeed, moveSpeed, 0.1f);

        _transform.Translate(new Vector3(_currentMoveSpeed, 0f, 0f));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void YAxisManipulation(float leftInput, float rightInput)
    {
        if (leftInput != rightInput)
        {
            var inputValue = Mathf.Max(rightInput, leftInput);
            ApplyGravity(inputValue);
            ApplyDrag(inputValue);
        }
        else if (rightInput >= 0.9f && leftInput >= 0.9f)
        {
            _rb.gravityScale = _maxNegativeGravity;
            ApplyDrag(1f);
        }
        else
        {
            _rb.gravityScale = _defaultGravityScale;
            _rb.drag = _minLinearDrag;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ApplyGravity(float inputValue)
    {
        _rb.gravityScale = _defaultGravityScale * SIDE_MOVE_GRAVITY_MULTIPLIER;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ApplyDrag(float inputValue)
    {
        var linearDrag = Mathf.Max(_maxLinearDrag * inputValue, _minLinearDrag);
        var currentDrag = Mathf.Lerp(_oldLinearDrag, linearDrag, 0.5f);
        _rb.drag = currentDrag;
        _oldLinearDrag = linearDrag;
    }

}
