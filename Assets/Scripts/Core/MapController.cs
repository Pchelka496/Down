using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public class MapController : MonoBehaviour
{
    LevelManager _levelManager;
    MapControllerConfig _config;

    string _currentPlatformAddress;
    CheckpointPlatform _currentPlatform;
    string _targetPlatformAddress;
    CheckpointPlatform _targetPlatform;

    public float FirstPlatformHeight
    {
        get
        {
            _config.GetFirstPlatformHeight();
            return 1f;
        }
    }

    [Inject]
    private void Construct(LevelManager levelManager)
    {
        _levelManager = levelManager;
    }

    public async void Initialize(MapControllerConfig config)
    {
        _config = config;
        var level = _config.GetLevel(_levelManager.PlayerSavedHeight);

        await UpdateCurrentPlatform(level);
        await UpdateTargetPlatform(level);

        await UpdateWorldBorder();
    }

    private async UniTask UpdateCurrentPlatform(MapControllerConfig.Level currentLevel)
    {
        if (_currentPlatformAddress == _config.PrefabCurrentCheckpointPlatformAddress) return;
        _currentPlatformAddress = _config.PrefabCurrentCheckpointPlatformAddress;
        var checkPoint = await LoadPrefabs(_currentPlatformAddress);

        if (checkPoint.TryGetComponent<CheckpointPlatform>(out var checkpointPlatform))
        {
            _currentPlatform = Instantiate(checkpointPlatform);
            InitializePlatform(_currentPlatform, currentLevel.CurrentPlatformHeight, currentLevel.CurrentPlatformWidth);
        }

    }

    private async UniTask UpdateTargetPlatform(MapControllerConfig.Level currentLevel)
    {
        if (_targetPlatformAddress == _config.PrefabTargetCheckpointPlatformAddress) return;

        _targetPlatformAddress = _config.PrefabTargetCheckpointPlatformAddress;
        var checkPoint = await LoadPrefabs(_targetPlatformAddress);

        if (checkPoint.TryGetComponent<CheckpointPlatform>(out var checkpointPlatform))
        {
            _targetPlatform = Instantiate(checkpointPlatform);
            InitializePlatform(_targetPlatform, currentLevel.TargetPlatformHeight, currentLevel.TargetPlatformWidth);
        }

    }

    private void InitializePlatform(CheckpointPlatform checkpointPlatform, float platformHeight, float platformWidth)
    {
        checkpointPlatform.Initialize(platformHeight, platformWidth);
    }

    private async UniTask<GameObject> LoadPrefabs(string address)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(address);

        await handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }
        else
        {
            Debug.LogError("Error loading via Addressables.");
            return default;
        }

    }

    private async UniTask UpdateWorldBorder()
    {
        var worldBorderPrefab = await LoadPrefabs(_config.PrefabWorldBorderAddress);
        var worldBorderSize = _config.WorldBorderSize; // Размеры каждой стены

        var currentLevel = _config.GetLevel(_levelManager.PlayerSavedHeight);
        float levelWidth = currentLevel.LevelWidth;
        float levelHeight = currentLevel.LevelHeight;

        // Получаем позиции текущей и таргет платформ
        float currentPlatformHeight = currentLevel.CurrentPlatformHeight;
        float targetPlatformHeight = currentLevel.TargetPlatformHeight;

        // Высота границ (между самой нижней платформой и самой верхней)
        float totalHeight = Mathf.Abs(currentPlatformHeight - targetPlatformHeight);

        // Позиции левой и правой стен
        Vector3 leftWallPosition = new Vector3(-levelWidth / 2, currentPlatformHeight + totalHeight / 2, 0);
        Vector3 rightWallPosition = new Vector3(levelWidth / 2, currentPlatformHeight + totalHeight / 2, 0);

        // Создаем левую стену
        var leftWall = Instantiate(worldBorderPrefab, leftWallPosition, Quaternion.identity);
        leftWall.transform.localScale = new Vector3(worldBorderSize.x, totalHeight, 1);

        // Создаем правую стену
        var rightWall = Instantiate(worldBorderPrefab, rightWallPosition, Quaternion.identity);
        rightWall.transform.localScale = new Vector3(worldBorderSize.x, totalHeight, 1);
    }

}
