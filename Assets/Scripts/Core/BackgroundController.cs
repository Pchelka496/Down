using System.Runtime.CompilerServices;
using UnityEngine;
using Zenject;

public class BackgroundController : MonoBehaviour
{
    const float MIN_HEIGHT = 0f;
    [SerializeField] Gradient _backgroundGradient;
    [SerializeField] Camera _camera;
    MapController _mapController;
    BackgroundControllerConfig _config;
    CharacterController _player;
    float _maxLevelHeight;

    [Inject]
    private void Construct(CharacterController player, MapController mapController)
    {
        _player = player;
        _mapController = mapController;
    }

    public void Initialize(BackgroundControllerConfig config)
    {
        _config = config;
    }

    private void Start()
    {
        _maxLevelHeight = _mapController.FullMapHeight;
    }

    private void Update()
    {
        float playerHeight = _player.transform.position.y;

        float normalizedHeight = Mathf.InverseLerp(_maxLevelHeight, MIN_HEIGHT, playerHeight);

        ChangeBackground(normalizedHeight);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ChangeBackground(float normalizedHeight)
    {
        Color backgroundColor = _backgroundGradient.Evaluate(normalizedHeight);

        _camera.backgroundColor = backgroundColor;
    }

}

