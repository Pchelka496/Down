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
    [SerializeField] UpgradeInfo _stabilizer;
    [SerializeField] UpgradeInfo _enginePower;
    [SerializeField] UpgradeInfo _engineAccelerationSpeed;
    [SerializeField] UpgradeInfo _outerSkinStrength;
    [SerializeField] UpgradeInfo _airBrake;
    [SerializeField] UpgradeInfo _emergencyBrakeSystem;
    [SerializeField] UpgradeInfo _boosterModule;
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

        _stabilizer.Initialize(_configs.StabilizationModuleConfig);
        _enginePower.Initialize(_configs.EngineModulePowerConfig);
        _engineAccelerationSpeed.Initialize(_configs.EngineModuleAccelerationSpeedConfig);
        _outerSkinStrength.Initialize(_configs.HealthModuleConfig);
        _airBrake.Initialize(_configs.AirBrakeModuleConfig);
        _emergencyBrakeSystem.Initialize(_configs.EmergencyBrakeModuleConfig);
        _boosterModule.Initialize(_configs.BoosterModuleConfig);
    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    private async void UpdateCurrentPoints()
    {
        var points = await _rewardManager.GetPoints();

        _currentPointsText.text = points.ToString();
    }

    //Buttons
    public void UpgradeStabilizer()
    {
        UpgradeLevel(_stabilizer.ModuleConfig, _stabilizer).Forget();
    }

    public void UpgradeEnginePower()
    {
        UpgradeLevel(_enginePower.ModuleConfig, _enginePower).Forget();
    }

    public void UpgradeEngineAccelerationSpeed()
    {
        UpgradeLevel(_engineAccelerationSpeed.ModuleConfig, _engineAccelerationSpeed).Forget();
    }

    public void UpgradeOuterSkinStrength()
    {
        UpgradeLevel(_outerSkinStrength.ModuleConfig, _outerSkinStrength).Forget();
    }

    public void UpgradeAirBrake()
    {
        UpgradeLevel(_airBrake.ModuleConfig, _airBrake).Forget();
    }

    public void UpgradeEmergencyBrakeSystem()
    {
        UpgradeLevel(_emergencyBrakeSystem.ModuleConfig, _emergencyBrakeSystem).Forget();
    }

    public void UpgradeBoosterPower()
    {
        UpgradeLevel(_boosterModule.ModuleConfig, _boosterModule).Forget();
    }
    //

    public async UniTaskVoid UpgradeLevel(BaseModuleConfig config, UpgradeInfo upgradeInfo)
    {
        var pointsNeeded = config.GetCurrentLevelCost();

        if (await _rewardManager.GetPoints() >= pointsNeeded && config.SetLevelCheck(config.GetCurrentLevel() + 1))
        {
            HandleSuccessfulUpgrade(config, upgradeInfo, pointsNeeded);
        }
        else
        {
            HandleUnsuccessfulUpgrade();
        }
    }

    private void HandleSuccessfulUpgrade(BaseModuleConfig config, UpgradeInfo upgradeInfo, int pointsNeeded)
    {
        _successfulSoundPlayer.PlayNextSound();

        _rewardManager.DecreasePoints(pointsNeeded);

        config.SetLevel(config.GetCurrentLevel() + 1);

        upgradeInfo.UpdateCurrentLevel(config.GetCurrentLevel());

        UpdateCurrentPoints();
    }

    private void HandleUnsuccessfulUpgrade()
    {
        _unsuccessfulSoundPlayer.PlayNextSound();
    }

}
