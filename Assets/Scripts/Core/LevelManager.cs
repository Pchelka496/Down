using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

public class LevelManager
{
    public const float PLAYER_START_Y_POSITION = 99989.1f;
    public const float PLAYER_START_X_POSITION = 0;

    public const string CONFIG_ADDRESS = "ScriptableObject/LevelManager/LevelManagerConfig";
    int _targetFrameRate = 90;
    LevelManagerConfig _config;

    EnemyManager _enemyManager;
    CharacterController _player;
    MapController _mapController;
    BackgroundController _backgroundController;
    PickUpItemManager _rewardManager;
    ScreenFader _screenFader;

    event Action<LevelManager> RoundStartAction;
    event Action<LevelManager, EnumRoundResults> RoundEndAction;

    bool _isRoundActive = false;

    public bool IsRoundActive => _isRoundActive;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(MapController mapController,
                           EnemyManager enemyManager,
                           BackgroundController backgroundController,
                           CharacterController player,
                           PickUpItemManager rewardManager,
                           ScreenFader screenFader
                           )
    {
        _mapController = mapController;
        _enemyManager = enemyManager;
        _backgroundController = backgroundController;
        _rewardManager = rewardManager;
        _screenFader = screenFader;
        _player = player;

        LoadConfig().Forget();
    }

    private async UniTask LoadConfig()
    {
        var loadOperationData = await AddressableLouderHelper.LoadAssetAsync<LevelManagerConfig>(CONFIG_ADDRESS);

        _config = loadOperationData.Handle.Result;

        if (_config == null)
        {
            Debug.LogError("Failed to load OptionalPlayerModuleLoaderConfig.");
            return;
        }

        InitializeAnyComponents();
        FirstSettings();
    }

    private void FirstSettings()
    {
        ResetPlayerPosition();
        _screenFader.FadeToClear().Forget();
        _targetFrameRate = _config.TargetFrameRate;

        Application.targetFrameRate = _targetFrameRate;
    }

    private void InitializeAnyComponents()
    {
        _mapController.Initialize(_config.MapControllerConfig);
        _backgroundController.Initialize(_config.BackgroundControllerConfig);
        _enemyManager.Initialize(_config.EnemyManagerConfig);
        _rewardManager.Initialize(_config.RewardManagerConfig);
    }

    private void ResetPlayerPosition()
    {
        _player.transform.position = new Vector2(PLAYER_START_X_POSITION, PLAYER_START_Y_POSITION);
        _player.Rb.velocity = Vector2.zero;
    }

    public async UniTaskVoid RoundStart()
    {
        await UniTask.WaitForSeconds(1f);
        _isRoundActive = true;

        if (RoundStartAction != null)
        {
            foreach (var handler in RoundStartAction.GetInvocationList())
            {
                try
                {
                    ((Action<LevelManager>)handler)?.Invoke(this);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in RoundStartAction handler: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        RoundStartAction = null;
    }

    public async UniTaskVoid RoundEnd()
    {
        await UniTask.WaitForSeconds(1f);
        _isRoundActive = false;

        if (RoundEndAction != null)
        {
            foreach (var handler in RoundEndAction.GetInvocationList())
            {
                try
                {
                    ((Action<LevelManager, EnumRoundResults>)handler)?.Invoke(this, EnumRoundResults.Positive);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in RoundEndAction handler: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        RoundEndAction = null;

        await _screenFader.FadeToBlack();

        ResetPlayerPosition();
        await UniTask.WaitForSeconds(1f);

        await _screenFader.FadeToClear();
    }


    public void SubscribeToRoundStart(Action<LevelManager> action) => RoundStartAction += action;
    public void SubscribeToRoundEnd(Action<LevelManager, EnumRoundResults> action) => RoundEndAction += action;

}
