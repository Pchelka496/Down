using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;
using static GameplaySceneInstaller;

public class PlayerUpgradePanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _currentPointsText;
    [SerializeField] SoundPlayerRandomPitch _unsuccessfulSoundPlayer;
    [SerializeField] SoundPlayerRandomPitch _successfulSoundPlayer;

    [SerializeField] EngineUpdater _engineUpdater;
    [SerializeField] HealthModuleUpdater _healthModuleUpdater;
    [SerializeField] PickerModuleUpdater _pieceModuleUpdater;
    [SerializeField] AirBrakeUpdater _airBrakeUpdater;
    [SerializeField] StabilizationModuleUpdater _stabilizationModuleUpdater;
    [SerializeField] EmergencyBrakeUpdater _emergencyBrakeUpdater;

    AudioSourcePool _audioSourcePool;
    PlayerModuleConfigs _configs;
    RewardManager _rewardManager;

    [Inject]
    private void Construct(RewardManager rewardManager, PlayerModuleConfigs configs, AudioSourcePool audioSourcePool)
    {
        _unsuccessfulSoundPlayer.Initialize(audioSourcePool);
        _successfulSoundPlayer.Initialize(audioSourcePool);

        _rewardManager = rewardManager;
        _audioSourcePool = audioSourcePool;
        _configs = configs;
    }

    private void Start()
    {
        ClosePanel();

        UpdateCurrentPoints();

        _engineUpdater.Initialize(_configs.EngineModuleConfig, this);
        _healthModuleUpdater.Initialize(_configs.HealthModuleConfig, this);
        _pieceModuleUpdater.Initialize(_configs.PickerModuleConfig, this);
        _airBrakeUpdater.Initialize(_configs.AirBrakeModuleConfig, this);
        _stabilizationModuleUpdater.Initialize(_configs.StabilizationModuleConfig, this);
        _emergencyBrakeUpdater.Initialize(_configs.EmergencyBrakeModuleConfig, this);
    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    private void UpdateCurrentPoints()
    {
        var points = _rewardManager.GetPoints();

        _currentPointsText.text = points.ToString();
    }

    public bool UpgradeLevelCheck(int pointsNeeded, int currentLevel, int maxLevel)
    {
        if (_rewardManager.GetPoints() >= pointsNeeded && currentLevel < maxLevel)
        {
            HandleSuccessfulUpgrade(pointsNeeded);
            return true;
        }
        else
        {
            HandleUnsuccessfulUpgrade();
            return false;
        }
    }

    public bool DowngradeLevelCheck(int currentLevel)
    {
        if (currentLevel >= 0)
        {
            HandleSuccessfulUpgrade(0);
            return true;
        }
        else
        {
            HandleUnsuccessfulUpgrade();
            return false;
        }
    }

    private void HandleSuccessfulUpgrade(int pointsNeeded)
    {
        _successfulSoundPlayer.PlayNextSound();

        _rewardManager.DecreasePoints(pointsNeeded);

        UpdateCurrentPoints();
    }

    private void HandleUnsuccessfulUpgrade()
    {
        _unsuccessfulSoundPlayer.PlayNextSound();
    }

}
