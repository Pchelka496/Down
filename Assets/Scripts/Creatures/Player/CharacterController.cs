using System.Runtime.CompilerServices;
using UnityEngine;
using Zenject;

public class CharacterController : MonoBehaviour
{
    const float MAX_INTROPALITES_INPUT_VALUE = 0.5f;
    const float MIN_INTROPALITES_INPUT_VALUE = 0.05f;

    [SerializeField] float _maxLinearDrag;
    [SerializeField] float _minLinearDrag;

    [SerializeField] Rigidbody2D _rb;
    [SerializeField] Collider2D _collider;
    [SerializeField] float _maxMoveSpeed = 0.1f;

    float _currentMoveSpeed = 0f;
    float _defaultGravityScale = 1f;

    Controls _controls;

    float _oldLeftInputValue;
    float _oldRightInputValue;

    float _oldLinearDrag;

    [Inject]
    private void Construct(Controls controls)
    {
        _controls = controls;
        _controls.Enable();
    }

    private void Awake()
    {
        _defaultGravityScale = _rb.gravityScale;
    }

    private void FixedUpdate()
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

        transform.Translate(new(_currentMoveSpeed, 0f, 0f));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void YAxisManipulation(float leftInput, float rightInput)
    {
        if (leftInput > rightInput)
        {
            ApplyGravityAndDrag(leftInput, Vector2.left);
        }
        else if (rightInput > leftInput)
        {
            ApplyGravityAndDrag(rightInput, Vector2.right);
        }
        else if (rightInput > 0f && leftInput > 0f)
        {
            _rb.gravityScale = 0f;
        }
        else
        {
            _rb.gravityScale = _defaultGravityScale;
            _rb.drag = _minLinearDrag;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ApplyGravityAndDrag(float inputValue, Vector2 direction)
    {
        _rb.gravityScale = _defaultGravityScale * 0.8f;

        var linearDrag = Mathf.Max(_maxLinearDrag * inputValue, _minLinearDrag);
        var currentDrag = Mathf.Clamp(_oldLinearDrag, linearDrag, 0.5f);

        _rb.drag = currentDrag;
        _oldLinearDrag = linearDrag;
    }

    private void Reset()
    {
        if (gameObject.TryGetComponent<Rigidbody2D>(out var rb))
        {
            _rb = rb;
        }
        else
        {
            _rb = gameObject.AddComponent<Rigidbody2D>();
        }
        if (gameObject.TryGetComponent<Collider2D>(out var collider))
        {
            _collider = collider;
        }
    }

}
