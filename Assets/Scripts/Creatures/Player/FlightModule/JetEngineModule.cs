using Cysharp.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using Zenject;

public class JetEngineModule : BaseModule, IFlightModule
{
    [Header("General Settings")]
    [SerializeField] private float maxThrust = 10f;  // Максимальная мощность
    [SerializeField] private float thrustIncreaseSpeed = 0.1f;  // Скорость прироста мощности

    [Header("Engine Transforms")]
    [SerializeField] private Transform leftEngine;  // Левый двигатель
    [SerializeField] private Transform rightEngine;  // Правый двигатель

    private Rigidbody2D _rb;
    private Controls _controls;

    private float _leftThrust;  // Мощность левого двигателя
    private float _rightThrust;  // Мощность правого двигателя
    private float _currentLeftThrust;  // Текущая мощность левого двигателя
    private float _currentRightThrust;  // Текущая мощность правого двигателя

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
            // Чтение значений ввода от пользователя
            _leftThrust = _controls.Player.MoveLeft.ReadValue<float>();
            _rightThrust = _controls.Player.MoveRight.ReadValue<float>();

            // Плавное увеличение мощности двигателей
            _currentLeftThrust = Mathf.Lerp(_currentLeftThrust, _leftThrust * maxThrust, thrustIncreaseSpeed);
            _currentRightThrust = Mathf.Lerp(_currentRightThrust, _rightThrust * maxThrust, thrustIncreaseSpeed);

            // Применение силы к каждому двигателю
            ApplyThrust(leftEngine, _currentLeftThrust);
            ApplyThrust(rightEngine, _currentRightThrust);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);  // Используем FixedUpdate для работы с физикой
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
        // Рисуем направления двигателей для наглядности в редакторе
        if (leftEngine != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(leftEngine.position, leftEngine.up * 2);  // Левый двигатель
        }

        if (rightEngine != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(rightEngine.position, rightEngine.up * 2);  // Правый двигатель
        }
    }
#endif

}
