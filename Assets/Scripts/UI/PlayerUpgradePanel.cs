using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;
using static GameplaySceneInstaller;

public class PlayerUpgradePanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _currentPointsText;
    [SerializeField] UpgradeInfo _stabilizer;
    [SerializeField] UpgradeInfo _enginePower;
    [SerializeField] UpgradeInfo _engineAccelerationSpeed;
    [SerializeField] UpgradeInfo _outerSkinStrength;
    [SerializeField] UpgradeInfo _airBrake;
    [SerializeField] UpgradeInfo _emergencyBrakeSystem;
    RewardManager _rewardManager;

    [Inject]
    private void Construct(RewardManager rewardManager, PlayerModuleConfigs configs)
    {
        _rewardManager = rewardManager;

        _stabilizer.Initialize(configs.StabilizationModuleConfig);
        _enginePower.Initialize(configs.EngineModulePowerConfig);
        _engineAccelerationSpeed.Initialize(configs.EngineModuleAccelerationSpeedConfig);
        _outerSkinStrength.Initialize(configs.HealthModuleConfig);
        _airBrake.Initialize(configs.AirBrakeModuleConfig);
        _emergencyBrakeSystem.Initialize(configs.EmergencyBrakeModuleConfig);
    }

    private void Start()
    {
        UpdateCurrentPoints();
    }

    private void OnEnable()
    {


    }

    private async void UpdateCurrentPoints()
    {
        var points = await _rewardManager.GetPoints();

        _currentPointsText.text = points.ToString();
    }

    public void UpgradeStabilizer()
    {
        UpdateLevel(_stabilizer.ModuleConfig, _stabilizer).Forget();
    }

    public void UpgradeEnginePower()
    {
        UpdateLevel(_enginePower.ModuleConfig, _enginePower).Forget();
    }

    public void UpgradeEngineAccelerationSpeed()
    {
        UpdateLevel(_engineAccelerationSpeed.ModuleConfig, _engineAccelerationSpeed).Forget();
    }

    public void UpgradeOuterSkinStrength()
    {
        UpdateLevel(_outerSkinStrength.ModuleConfig, _outerSkinStrength).Forget();
    }

    public void UpgradeAirBrake()
    {
        UpdateLevel(_airBrake.ModuleConfig, _airBrake).Forget();
    }

    public void UpdateStatusEmergencyBrakeSystem()
    {
        UpdateLevel(_emergencyBrakeSystem.ModuleConfig, _emergencyBrakeSystem).Forget();
    }

    public async UniTaskVoid UpdateLevel(BaseModuleConfig config, UpgradeInfo upgradeInfo)
    {
        var pointsNeeded = config.GetCurrentLevelCost();

        if (await _rewardManager.GetPoints() >= pointsNeeded && config.SetLevelCheck(config.GetCurrentLevel() + 1))
        {
            HandleSufficientPoints(config, upgradeInfo, pointsNeeded);
        }
        else
        {
            HandleInsufficientPoints();
        }
    }

    private void HandleSufficientPoints(BaseModuleConfig config, UpgradeInfo upgradeInfo, int pointsNeeded)
    {
        _rewardManager.DecreasePoints(pointsNeeded);

        config.SetLevel(config.GetCurrentLevel() + 1);

        upgradeInfo.UpdateCurrentLevel(config.GetCurrentLevel());

        UpdateCurrentPoints();
    }

    private void HandleInsufficientPoints()
    {
        Debug.Log("Not enough points to upgrade the level.");
    }

}
