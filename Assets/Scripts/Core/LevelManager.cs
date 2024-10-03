using UnityEngine;
using Zenject;

public class LevelManager : MonoBehaviour
{
    [SerializeField] LevelManagerConfig _config;
    CharacterController _player;
    MapController _mapController;
    BackgroundController _backgroundController;

    private float CurrentSavedHeight
    {
        get
        {
            return _config.PlayerSavedHeight;
        }
        set
        {
            _config.PlayerSavedHeight = value;
        }
    }

    public float PlayerSavedHeight
    {
        get
        {
            var value = _config.PlayerSavedHeight;

            if (float.IsNaN(value))
            {
                return _mapController.FirstHeight;
            }
            else
            {
                return _config.PlayerSavedHeight;
            }
        }
        set
        {
            _config.PlayerSavedHeight = value;
        }
    }

    [Inject]
    private void Construct(MapController mapController, BackgroundController backgroundController, CharacterController player)
    {
        _mapController = mapController;
        _backgroundController = backgroundController;
        _player = player;
    }

    private void Awake()
    {
        _mapController.Initialize(_config.MapControllerConfig);
        _backgroundController.Initialize(_config.BackgroundControllerConfig);
    }

    private void Start()
    {
        _player.transform.position = new(0f, PlayerSavedHeight);
    }

    public void StartNextLevel()
    {
        //_mapController.St
         //_backgroundController.
    }

    public void SwitchToNextLevel()
    {
         _mapController.SwitchToNextLevel();
    }

}
