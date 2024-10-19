using Cysharp.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using Zenject;

public class CharacterController : MonoBehaviour
{
    public const float PLAYER_RADIUS = 0.7f;
    const int LAYER_MASK = 1 << 11;

    [SerializeField] Transform _bodySprite;
    [SerializeField] Rigidbody2D _rb;
    [SerializeField] CircleCollider2D _collider;

    IFlightModule _flightModule;
    IGroundMovementModule _groundMovementModule;

    // CancellationTokenSource _flightModeTransfer;
    CancellationTokenSource _flightCts;
    CancellationTokenSource _groundMovementCts;

    Controls _controls;

    public Rigidbody2D Rb { get => _rb; set => _rb = value; }
    public Transform BodySprite { get => _bodySprite; set => _bodySprite = value; }
    public CircleCollider2D Collider { get => _collider; set => _collider = value; }

    public IFlightModule FlightModule
    {
        get => _flightModule; set
        {
            TokenClearing(ref _flightCts);
            _flightModule = value;

            SetFlightControl();
        }
    }

    public IGroundMovementModule GroundMovementModule
    {
        get => _groundMovementModule; set
        {
            TokenClearing(ref _groundMovementCts);
            _groundMovementModule = value;

            SetGroundMovement();
        }
    }

    [Inject]
    private void Construct(Controls controls, LevelManager levelManager)
    {
        _controls = controls;
        _controls.Enable();
        //levelManager.SubscribeToRoundStart(RoundStart);
    }

    //private void RoundStart(LevelManager levelManager)
    //{
    //    levelManager.SubscribeToRoundEnd(RoundEnd);
    //}

    //private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    //{
    //    levelManager.SubscribeToRoundStart(RoundStart);
    //}

    //private void OnCollisionEnter2D(Collision2D collision)
    //{

    //}

    //private void OnCollisionExit2D(Collision2D collision)
    //{

    //}

    private void SetGroundMovement()
    {
        TokenClearing(ref _groundMovementCts);
        TokenClearing(ref _flightCts);

        _groundMovementCts = new();
        GroundMovementModule.GroundMovement(_groundMovementCts).Forget();
    }

    private void SetFlightControl()
    {
        TokenClearing(ref _groundMovementCts);
        TokenClearing(ref _flightCts);

        _flightCts = new();
        FlightModule.Fly(_flightCts).Forget();
    }

    private void TokenClearing(ref CancellationTokenSource cancellationToken)
    {
        if (cancellationToken != null)
        {
            cancellationToken.Cancel();
            cancellationToken.Dispose();
            cancellationToken = null;
        }
    }

    private async UniTask GroundMovement(CancellationTokenSource cancellationToken)
    {
        await UniTask.CompletedTask;
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
            Collider = collider;
        }
    }

    private void OnDestroy()
    {
        if (_flightCts != null)
        {
            TokenClearing(ref _flightCts);
        }
        if (_groundMovementCts != null)
        {
            TokenClearing(ref _groundMovementCts);
        }
    }

}
