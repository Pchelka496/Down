using Cysharp.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using Zenject;

public class CharacterController : MonoBehaviour
{
    const int LAYER_MASK = 1 << 11;

    const float SET_FLYGHT_CONTROL_DELAY = 1f;
    const float SIDE_MOVE_GRAVITY_MULTIPLIER = 0.8f;
    const float MAX_INTROPALITES_INPUT_VALUE = 0.5f;
    const float MIN_INTROPALITES_INPUT_VALUE = 0.001f;
    const float JUMP_RELOAD = 0.3f;

    [SerializeField] Transform _groundCheck;
    [SerializeField] Transform _bodySprite;
    [SerializeField] Rigidbody2D _rb;
    [SerializeField] CircleCollider2D _collider;
    [SerializeField] float _groundCheckDistance = 0.2f;
    [SerializeField] float _jumpForce = 5f;

    [SerializeField] float _maxMoveSpeed = 5f;

    IFlightModule _flightModule;

    bool _onGround;
    CancellationTokenSource _flightModeTransfer;
    CancellationTokenSource _flightCancellationToken;
    CancellationTokenSource _groundMovementCancellationToken;

    Controls _controls;
    float _radius;
    float _defaultGravityScale = 1f;
    float _oldLeftInputValue;
    float _oldRightInputValue;
    float _oldLinearDrag;

    bool _jumpReloaded = true;

    public Rigidbody2D Rb { get => _rb; set => _rb = value; }
    public IFlightModule FlightModule { get => _flightModule; set => _flightModule = value; }

    [Inject]
    private void Construct(Controls controls)
    {
        _controls = controls;
        _controls.Enable();
    }

    private void Awake()
    {
        _defaultGravityScale = _rb.gravityScale;
        _flightModule = GameplaySceneInstaller.DiContainer.Instantiate<FlightModule>();
        _radius = _collider.radius;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_flightModeTransfer != null)
        {
            TokenClearing(ref _flightModeTransfer);
        }
        if (_flightCancellationToken != null)
        {
            TokenClearing(ref _flightCancellationToken);
        }
        if (_groundMovementCancellationToken != null)
        {
            TokenClearing(ref _groundMovementCancellationToken);
        }

        _groundMovementCancellationToken = new();
        GroundMovement(_groundMovementCancellationToken).Forget();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (_flightModeTransfer != null)
        {
            TokenClearing(ref _flightModeTransfer);
        }

        _flightModeTransfer = new();
        SetFlightControl(_flightModeTransfer).Forget();
    }

    private async UniTask SetFlightControl(CancellationTokenSource cancellationToken)
    {
        await UniTask.WaitForSeconds(SET_FLYGHT_CONTROL_DELAY);

        if (cancellationToken.IsCancellationRequested) return;

        if (GroundCheck())
        {
            SetFlightControl(cancellationToken).Forget();
            return;
        }

        if (_groundMovementCancellationToken != null)
        {
            TokenClearing(ref _groundMovementCancellationToken);
        }
        if (_flightCancellationToken != null)
        {
            TokenClearing(ref _flightCancellationToken);
        }

        _flightCancellationToken = new();
        _flightModule.Fly(_flightCancellationToken).Forget();
    }

    private void TokenClearing(ref CancellationTokenSource cancellationToken)
    {
        cancellationToken.Cancel();
        cancellationToken.Dispose();
        cancellationToken = null;
    }

    private async UniTask GroundMovement(CancellationTokenSource cancellationToken)
    {
        var currentMoveSpeed = 0f;
        var oldLeftInputValue = 0f;
        var oldRightInputValue = 0f;

        while (!cancellationToken.IsCancellationRequested)
        {
            var newLeftInput = _controls.Player.MoveLeft.ReadValue<float>();
            var newRightInput = _controls.Player.MoveRight.ReadValue<float>();

            var leftInput = Mathf.Lerp(oldLeftInputValue, newLeftInput, MIN_INTROPALITES_INPUT_VALUE);
            var rightInput = Mathf.Lerp(oldRightInputValue, newRightInput, MIN_INTROPALITES_INPUT_VALUE);

            if (newLeftInput >= 0.9f && newRightInput >= 0.9f)
            {
                JumpIfPossible();
            }

            var moveSpeed = _maxMoveSpeed * (Mathf.Max(newLeftInput, newRightInput) - Mathf.Min(newLeftInput, newRightInput));
            if (newLeftInput > newRightInput)
            {
                moveSpeed = -moveSpeed;
            }

            currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, moveSpeed, 0.1f);

            transform.Translate(new Vector3(currentMoveSpeed, 0f, 0f));

            var rotationRate = (Mathf.Abs(currentMoveSpeed) / _radius) * Mathf.Rad2Deg;

            if (currentMoveSpeed > 0)
            {
                rotationRate = -rotationRate;
            }

            _bodySprite.Rotate(0, 0, rotationRate);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool JumpIfPossible()
    {
        var groundCheck = GroundCheck();
        var feasibility = groundCheck & _jumpReloaded;

        if (feasibility)
        {
            _jumpReloaded = false;
            JumpReload();
            Jump();
        }

        return feasibility;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GroundCheck()
    {
        var hit = Physics2D.Raycast(_groundCheck.position, Vector2.down, _groundCheckDistance, LAYER_MASK);

        if (hit.collider == null)
        {
            return false;
        }

        if (hit.collider.isTrigger)
        {
            return false;
        }

        return true;
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
        if (gameObject.TryGetComponent<CircleCollider2D>(out var collider))
        {
            _collider = collider;
        }
    }

    private void OnDestroy()
    {
        if (_flightModeTransfer != null)
        {
            TokenClearing(ref _flightModeTransfer);
        }
        if (_flightCancellationToken != null)
        {
            TokenClearing(ref _flightCancellationToken);
        }
        if (_groundMovementCancellationToken != null)
        {
            TokenClearing(ref _groundMovementCancellationToken);
        }
    }

#if UNITY_EDITOR
    public Transform GroundCheckEditor => _groundCheck;
    public float GroundCheckDistanceEditor => _groundCheckDistance;
#endif

}
