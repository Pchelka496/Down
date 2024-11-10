using Cysharp.Threading.Tasks;
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
    [SerializeField] MultiTargetRotationFollower _follower;
    [SerializeField] HealthModule _healthModule;

    IFlightModule _flightModule;

    public Rigidbody2D Rb { get => _rb; set => _rb = value; }
    public CircleCollider2D Collider { get => _collider; set => _collider = value; }
    public MultiTargetRotationFollower MultiTargetRotationFollower => _follower;
    public HealthModule HealthModule { get => _healthModule; }
    public EmergencyBrakeModule EmergencyBrakeModule { get; set; }

    public IFlightModule FlightModule
    {
        get => _flightModule; set
        {
            _flightModule = value;
        }
    }

    [Inject]
    private void Construct(Controls controls, LevelManager levelManager)
    {
        controls.Enable();
        SetLobbyMode();
        levelManager.SubscribeToRoundStart(RoundStart);
    }

    private void RoundStart(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundEnd(RoundEnd);
        SetGameplayMode();
    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
        SetLobbyMode();
    }

    private void SetLobbyMode()
    {
        _rb.gravityScale = 0f;

    }

    private void SetGameplayMode()
    {
        _rb.gravityScale = 1f;

    }

    private void TokenClearing(ref CancellationTokenSource cancellationToken)
    {
        if (cancellationToken != null)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.Cancel();
            }
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

}
