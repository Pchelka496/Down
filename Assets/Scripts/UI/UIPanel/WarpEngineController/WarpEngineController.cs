using Additional;
using Creatures.Player;
using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;

public class WarpEngineController : MonoBehaviour, IUIPanel
{
    [SerializeField] CustomPressButton _startWarpMoving;
    [SerializeField] CustomPressButton _increaseSpentEnergy;
    [SerializeField] CustomPressButton _decreaseSpentEnergy;
    [SerializeField] TextMeshProUGUI _costText;

    [SerializeField] LevelProgressDisplay _chargingLevelDisplay;

    WarpEngineModule _module;
    WarpEngineModuleConfig _moduleConfig;

    PlayerResourcedKeeper _resourcedKeeper;
    CameraFacade _cameraFacade;
    GlobalEventsManager _globalEventsManager;
    PlayerController _player;

    CancellationTokenSource _updateTextCts;

    private int CurrentNumberOfChargingLevels
    {
        get => _moduleConfig.CurrentNumberOfChargingLevels;
        set
        {
            _moduleConfig.CurrentNumberOfChargingLevels = value;
            var newValue = _moduleConfig.CurrentNumberOfChargingLevels;

            ClearToken();
            _updateTextCts = new();

            _costText.text = newValue.ToString();

            _chargingLevelDisplay.UpdateLevelProgress(newValue);
        }
    }

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<ќжидание>")]
    private void Construct(CameraFacade cameraFacade,
                           WarpEngineModuleConfig moduleConfig,
                           PlayerResourcedKeeper playerResourcedKeeper,
                           PlayerController player,
                           GlobalEventsManager globalEventsManager)
    {
        _player = player;
        _cameraFacade = cameraFacade;
        _globalEventsManager = globalEventsManager;
        _resourcedKeeper = playerResourcedKeeper;
        _moduleConfig = moduleConfig;
        _module = player.WarpEngineModule;
    }

    private async void Start()
    {
        _startWarpMoving.onClick.AddListener(StartWarpMoving);
        _increaseSpentEnergy.onClick.AddListener(IncreaseSpentEnergy);
        _decreaseSpentEnergy.onClick.AddListener(DecreaseSpentEnergy);

        await _chargingLevelDisplay.Initialize(_moduleConfig.MaxNumberOfChargingLevels);

        CurrentNumberOfChargingLevels = _moduleConfig.CurrentNumberOfChargingLevels;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        _player.CloseUIPanel();
    }

    void IUIPanel.Open()
    {
        gameObject.SetActive(true);
        _player.OpenUIPanel();
    }

    private async void StartWarpMoving()
    {
        if (!_resourcedKeeper.TryDecreaseEnergy(CurrentNumberOfChargingLevels, true)) return;

        Close();

        var targetHeight = CalculateTargetHeight(_moduleConfig.CurrentNumberOfChargingLevels);
        var moveDuration = CalculateMoveDuration(targetHeight);

        _globalEventsManager.StartWarpMoving();

        await UniTask.WaitUntil(() => _cameraFacade.OnChangeTargetAnimationEnding);

        await _module.StartMoving(targetHeight, moveDuration);

        _globalEventsManager.WarpEnd();
    }

    private float CalculateTargetHeight(int currentChargingLevel)
    {
        var minHeight = _moduleConfig.MinHeight;
        var maxHeight = _moduleConfig.MaxHeight;
        var maxChargingLevel = _moduleConfig.MaxNumberOfChargingLevels;

        return Mathf.Lerp(minHeight, maxHeight, (float)currentChargingLevel / maxChargingLevel);
    }

    private float CalculateMoveDuration(float targetHeight)
    {
        var minDuration = _moduleConfig.MinMoveDuration;
        var maxDuration = _moduleConfig.MaxMoveDuration;

        var minHeight = _moduleConfig.MinHeight;
        var maxHeight = _moduleConfig.MaxHeight;

        float normalizedHeight = (targetHeight - minHeight) / (maxHeight - minHeight);

        return Mathf.Lerp(minDuration, maxDuration, normalizedHeight);
    }

    private void IncreaseSpentEnergy()
    {
        CurrentNumberOfChargingLevels++;
    }

    private void DecreaseSpentEnergy()
    {
        CurrentNumberOfChargingLevels--;
    }

    private void ClearToken() => ClearTokenSupport.ClearToken(ref _updateTextCts);

    private void OnDestroy()
    {
        ClearToken();
    }
}
