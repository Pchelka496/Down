using Cysharp.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using Zenject;

public class JetEngineModule : BaseModule, IFlightModule
{
    [Header("General Settings")]
    [SerializeField] private float maxThrust = 10f;  // ������������ ��������
    [SerializeField] private float thrustIncreaseSpeed = 0.1f;  // �������� �������� ��������

    [Header("Engine Transforms")]
    [SerializeField] private Transform leftEngine;  // ����� ���������
    [SerializeField] private Transform rightEngine;  // ������ ���������

    private Rigidbody2D _rb;
    private Controls _controls;

    private float _leftThrust;  // �������� ������ ���������
    private float _rightThrust;  // �������� ������� ���������
    private float _currentLeftThrust;  // ������� �������� ������ ���������
    private float _currentRightThrust;  // ������� �������� ������� ���������

    [Inject]
    private void Construct(CharacterController player, Controls controls)
    {
        _rb = player.Rb;
        _controls = controls;
        transform.parent = player.transform;
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        player.FlightModule = this;
    }

    public async UniTask Fly(CancellationTokenSource cts)
    {
        while (!cts.IsCancellationRequested)
        {
            // ������ �������� ����� �� ������������
            _leftThrust = _controls.Player.MoveLeft.ReadValue<float>();
            _rightThrust = _controls.Player.MoveRight.ReadValue<float>();

            // ������� ���������� �������� ����������
            _currentLeftThrust = Mathf.Lerp(_currentLeftThrust, _leftThrust * maxThrust, thrustIncreaseSpeed);
            _currentRightThrust = Mathf.Lerp(_currentRightThrust, _rightThrust * maxThrust, thrustIncreaseSpeed);

            // ���������� ���� � ������� ���������
            ApplyThrust(leftEngine, _currentLeftThrust);
            ApplyThrust(rightEngine, _currentRightThrust);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);  // ���������� FixedUpdate ��� ������ � �������
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ApplyThrust(Transform engine, float thrust)
    {
        float engineRotationZ = engine.eulerAngles.z - 90f + 180f;

        Vector2 forceDirection = new Vector2(-Mathf.Cos(engineRotationZ * Mathf.Deg2Rad), -Mathf.Sin(engineRotationZ * Mathf.Deg2Rad));

        _rb.AddForceAtPosition(forceDirection * thrust, engine.position);
        Debug.DrawRay(engine.position, forceDirection * thrust, Color.red, 0.1f);

    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // ������ ����������� ���������� ��� ����������� � ���������
        if (leftEngine != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(leftEngine.position, leftEngine.up * 2);  // ����� ���������
        }

        if (rightEngine != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(rightEngine.position, rightEngine.up * 2);  // ������ ���������
        }
    }
#endif

}
