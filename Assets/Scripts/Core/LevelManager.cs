using UnityEngine;
using Zenject;

public class LevelManager : MonoBehaviour
{
    [SerializeField] LevelManagerConfig _config;
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

    [Inject]
    private void Construct(MapController mapController, BackgroundController backgroundController)
    {
        _mapController = mapController;
        _backgroundController = backgroundController;
    }

    private void Awake()
    {
        _mapController.Initialize(_config.MapControllerConfig);
        _backgroundController.Initialize(_config.BackgroundControllerConfig);
    }

    public float PlayerSavedHeight
    {
        get
        {
            var value = _config.PlayerSavedHeight;

            if (value == LevelManagerConfig.NO_SAVED_HEIGHT)
            {
                return _mapController.FirstPlatformHeight;
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

}
