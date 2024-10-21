using Cysharp.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using Zenject;

public class JetEngineModule : BaseModule, IFlightModule
{
    [Header("General Settings")]
    [SerializeField] float maxThrust = 10f;
    [SerializeField] float thrustIncreaseSpeed = 0.1f;

    [Header("Engine Transforms")]
    [SerializeField] Transform _leftEngine;
    [SerializeField] Transform _rightEngine;

    [SerializeField] ParticleSystem _leftParticleSystem;
    [SerializeField] ParticleSystem _rightParticleSystem;

    Rigidbody2D _rb;
    Controls _controls;

    float _leftThrust;
    float _rightThrust;
    float _currentLeftThrust;
    float _currentRightThrust;

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
            ApplyThrust(_leftEngine, _currentLeftThrust);
            ApplyThrust(_rightEngine, _currentRightThrust);

            UpdateEngineParticles(_leftParticleSystem, _currentLeftThrust);
            UpdateEngineParticles(_rightParticleSystem, _currentRightThrust);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate); 
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

    private void UpdateEngineParticles(ParticleSystem particleSystem, float currentThrust)
    {
        var emission = particleSystem.emission;
        var main = particleSystem.main;

        emission.rateOverTime = Mathf.Lerp(0, 20f, currentThrust / maxThrust);

        main.startSpeed = Mathf.Lerp(2f, 10f, currentThrust / maxThrust);

        if (currentThrust > 0 && !particleSystem.isPlaying)
        {
            particleSystem.Play();
        }
        else if (currentThrust <= 0 && particleSystem.isPlaying)
        {
            particleSystem.Stop();
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Рисуем направления двигателей для наглядности в редакторе
        if (_leftEngine != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(_leftEngine.position, _leftEngine.up * 2);  // Левый двигатель
        }

        if (_rightEngine != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(_rightEngine.position, _rightEngine.up * 2);  // Правый двигатель
        }
    }
#endif

}
