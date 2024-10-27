using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;

public class LevelManager : MonoBehaviour
{
    public const float PLAYER_START_Y_POSITION = 99999f;
    public const float PLAYER_START_X_POSITION = 0;

    [SerializeField] LevelManagerConfig _config;
    EnemyManager _enemyManager;
    CharacterController _player;
    MapController _mapController;
    BackgroundController _backgroundController;
    RewardManager _rewardManager;
    ScreenFader _screenFader;

    event Action<LevelManager> _roundStartAction;
    event Action<LevelManager, EnumRoundResults> _roundEndAction;

    [Inject]
    private void Construct(MapController mapController, EnemyManager enemyManager, BackgroundController backgroundController, CharacterController player, RewardManager rewardManager, ScreenFader screenFader)
    {
        _mapController = mapController;
        _enemyManager = enemyManager;
        _backgroundController = backgroundController;
        _rewardManager = rewardManager;
        _screenFader = screenFader;
        _player = player;
    }

    private void Awake()
    {
        _mapController.Initialize(_config.MapControllerConfig);
        _backgroundController.Initialize(_config.BackgroundControllerConfig);
        _enemyManager.Initialize(_config.EnemyManagerConfig);
        _rewardManager.Initialize(_config.RewardManagerConfig);

    }

    private void Start()
    {
        ResetPlayerPosition();
        _screenFader.FadeToClear().Forget();
    }

    private void ResetPlayerPosition()
    {
        _player.transform.position = new(PLAYER_START_X_POSITION, PLAYER_START_Y_POSITION);
        _player.Rb.velocity = Vector2.zero;
    }

    public async UniTaskVoid RoundStart()
    {
        await UniTask.WaitForSeconds(1f);

        _roundStartAction?.Invoke(this);
        _roundStartAction = null;
    }

    public async UniTaskVoid RoundEnd()
    {
        await UniTask.WaitForSeconds(1f);

        _roundEndAction?.Invoke(this, EnumRoundResults.Positive);
        _roundEndAction = null;

        await _screenFader.FadeToBlack();

        ResetPlayerPosition();

        await _screenFader.FadeToClear();
    }

    public void SubscribeToRoundStart(Action<LevelManager> action) => _roundStartAction += action;
    public void SubscribeToRoundEnd(Action<LevelManager, EnumRoundResults> action) => _roundEndAction += action;//bool is win status

}
