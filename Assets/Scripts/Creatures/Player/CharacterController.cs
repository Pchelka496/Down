using Cysharp.Threading.Tasks;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class CharacterController : MonoBehaviour
{
    const float SIDE_MOVE_GRAVITY_MULTIPLIER = 0.8f;
    const float MAX_INTROPALITES_INPUT_VALUE = 0.5f;
    const float MIN_INTROPALITES_INPUT_VALUE = 0.05f;
    const float JUMP_RELOAD = 0.5f;

    [SerializeField] Transform _groundCheck;
    [SerializeField] float _groundCheckDistance = 0.2f;
    [SerializeField] float _jumpForce = 5f;

    [SerializeField][Range(-10f, 0f)] float _maxNegativeGravity = -0.5f;
    [SerializeField] float _maxLinearDrag;
    [SerializeField] float _minLinearDrag;

    [SerializeField] Rigidbody2D _rb;
    [SerializeField] Collider2D _collider;
    [SerializeField] float _maxMoveSpeed = 0.1f;

    Controls _controls;
    float _currentMoveSpeed = 0f;
    float _defaultGravityScale = 1f;
    float _oldLeftInputValue;
    float _oldRightInputValue;
    float _oldLinearDrag;

    bool _jumpReloaded = true;

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

            if (JumpFeasibilityCheck())
            {
                Jump();
            }
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool JumpFeasibilityCheck()
    {
        int layerMask = 1 << 11;
        var hit = Physics2D.Raycast(_groundCheck.position, Vector2.down, _groundCheckDistance, layerMask);

        if (hit.collider == null) return false;

        var groundCheck = true;
        var feasibility = groundCheck & _jumpReloaded;

        _jumpReloaded = false;

        if (feasibility)
        {
            JumpReload();
        }

        return feasibility;
    }

    private async void JumpReload()
    {
        await UniTask.WaitForSeconds(JUMP_RELOAD);
        _jumpReloaded = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Jump()
    {
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
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

#if UNITY_EDITOR
    public Transform GroundCheck => _groundCheck;
    public float GroundCheckDistance => _groundCheckDistance;
#endif

}
