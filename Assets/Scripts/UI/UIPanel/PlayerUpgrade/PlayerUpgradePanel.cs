using TMPro;
using UnityEngine;
using Zenject;
using static GameplaySceneInstaller;

[RequireComponent(typeof(UpgradePanelVisualController))]
public class PlayerUpgradePanel : MonoBehaviour, IUIPanel
{
    [SerializeField] RectTransform _baseRectTransform;
    [SerializeField] UpgradePanelVisualController _visualController;

    [SerializeField] TextMeshProUGUI _currentPointsText;
    [SerializeField] SoundPlayerRandomPitch _unsuccessfulSoundPlayer;
    [SerializeField] SoundPlayerRandomPitch _successfulSoundPlayer;

    [SerializeField] EngineUpdater _engineUpdater;
    [SerializeField] HealthModuleUpdater _healthModuleUpdater;
    [SerializeField] PickerModuleUpdater _pieceModuleUpdater;
    [SerializeField] AirBrakeUpdater _airBrakeUpdater;
    [SerializeField] RotationModuleUpdater _stabilizationModuleUpdater;
    [SerializeField] EmergencyBrakeUpdater _emergencyBrakeUpdater;

    [SerializeField] RectTransform _playerViewUpgradePosition;

    PlayerController _player;
    PlayerModuleConfigs _configs;
    RewardKeeper _rewardKeeper;

    public UpgradePanelVisualController VisualController => _visualController;
    public PlayerController Player => _player;
    public Vector2 PlayerViewUpgradePosition => _playerViewUpgradePosition.position;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(RewardKeeper rewardKeeper, PlayerModuleConfigs configs, AudioSourcePool audioSourcePool, PlayerController player)
    {
        _unsuccessfulSoundPlayer.Initialize(audioSourcePool);
        _successfulSoundPlayer.Initialize(audioSourcePool);

        _rewardKeeper = rewardKeeper;
        _configs = configs;
        _player = player;
    }

    private void Start()
    {
        UpdateCurrentPoints();

        _engineUpdater.Initialize(_configs.EngineModuleConfig, this);
        _healthModuleUpdater.Initialize(_configs.HealthModuleConfig, this);
        _pieceModuleUpdater.Initialize(_configs.PickerModuleConfig, this);
        _airBrakeUpdater.Initialize(_configs.AirBrakeModuleConfig, this);
        _stabilizationModuleUpdater.Initialize(_configs.StabilizationModuleConfig, this);
        _emergencyBrakeUpdater.Initialize(_configs.EmergencyBrakeModuleConfig, this);
        AdjustPanelElementForSafeArea(_baseRectTransform);
    }

    private void AdjustPanelElementForSafeArea(RectTransform rectTransform)
    {
        var safeArea = Screen.safeArea;

        var safeAreaMin = new Vector2(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height);
        var safeAreaMax = new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);

        rectTransform.anchorMin = new(rectTransform.anchorMin.x, safeAreaMin.y);
        rectTransform.anchorMax = new(rectTransform.anchorMax.x, safeAreaMax.y);

        var offsetMin = rectTransform.offsetMin;
        var offsetMax = rectTransform.offsetMax;

        offsetMin.y = Mathf.Max(offsetMin.y, 0);
        offsetMax.y = Mathf.Min(offsetMax.y, 0);

        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;
    }

    void IUIPanel.Open()
    {
        gameObject.SetActive(true);
        _player.OpenPanel();

        MovePlayerToUpgradeView();
    }

    void IUIPanel.Close()
    {
        gameObject.SetActive(false);
        _player.ClosePanel();
    }

    public void MovePlayerToUpgradeView()
    {
        _player.Rb.velocity = Vector3.zero;

        _player.transform.position = _playerViewUpgradePosition.position;
    }

    public void OnBackButtonClick()
    {
        switch (_visualController.CurrentViewMode)
        {
            case UpgradePanelVisualController.ViewMode.Basic:
                {
                    ((IUIPanel)this).Close();

                    break;
                }
            case UpgradePanelVisualController.ViewMode.Detailed:
                {
                    _visualController.EndViewDetailedInformation();

                    break;
                }
        }
    }

    private void UpdateCurrentPoints()
    {
        var points = _rewardKeeper.GetPoints();

        _currentPointsText.text = points.ToString();
    }

    public bool UpgradeLevelCheck(int pointsNeeded, int currentLevel, int maxLevel)
    {
        if (_rewardKeeper.GetPoints() >= pointsNeeded && currentLevel < maxLevel)
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

        _rewardKeeper.DecreasePoints(pointsNeeded);

        UpdateCurrentPoints();
    }

    private void HandleUnsuccessfulUpgrade()
    {
        _unsuccessfulSoundPlayer.PlayNextSound();
    }

}
