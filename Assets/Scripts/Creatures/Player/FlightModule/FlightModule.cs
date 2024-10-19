using Cysharp.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using Zenject;

public class FlightModule : BaseModule, IFlightModule
{
    const float MAX_INTROPALITES_INPUT_VALUE = 0.5f;
    const float MIN_INTROPALITES_INPUT_VALUE = 0.05f;

    float _maxLinearDrag = 5f;
    float _minLinearDrag = 0.2f;

    float _maxMoveSpeed = 0.1f;

    Controls _controls;
    float _currentMoveSpeed = 0f;

    float _oldLeftInputValue;
    float _oldRightInputValue;
    float _oldLinearDrag;

    Rigidbody2D _rb;
    Transform _transform;

    [Inject]
    private void Construct(CharacterController player, Controls controls)
    {
        _rb = player.Rb;
        _rb.freezeRotation = false;

        _transform = player.transform;
        _controls = controls;
        transform.parent = player.transform;

        player.FlightModule = this;
    }

    public async UniTask Fly(CancellationTokenSource cts)
    {
        while (!cts.IsCancellationRequested)
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
            ApplyDrag(inputValue);
        }
        else if (rightInput >= 0.9f && leftInput >= 0.9f)
        {
            ApplyDrag(1f);
        }
        else
        {
            _rb.drag = _minLinearDrag;
        }
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
