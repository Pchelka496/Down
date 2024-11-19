using TMPro;
using UnityEngine;
using Zenject;
using static GameplaySceneInstaller;

[RequireComponent(typeof(UpgradePanelVisualController))]
public class PlayerUpgradePanel : MonoBehaviour
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

    CharacterController _player;
    PlayerModuleConfigs _configs;
    PickUpItemManager _rewardManager;

    public UpgradePanelVisualController VisualController { get => _visualController; }
    public CharacterController Player => _player;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(PickUpItemManager rewardManager, PlayerModuleConfigs configs, AudioSourcePool audioSourcePool, CharacterController player)
    {
        _unsuccessfulSoundPlayer.Initialize(audioSourcePool);
        _successfulSoundPlayer.Initialize(audioSourcePool);

        _rewardManager = rewardManager;
        _configs = configs;
        _player = player;
    }

    private void Start()
    {
        UpdateCurrentPoints();
        ClosePanel();

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

    public void OpenPanel()
    {
        gameObject.SetActive(true);
        _player.OpenUpgradePanel();

        MovePlayerToUpgradeView();
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
                    ClosePanel();

                    break;
                }
            case UpgradePanelVisualController.ViewMode.Detailed:
                {
                    _visualController.EndViewDetailedInformation();

                    break;
                }
        }
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
        _player.CloseUpgradePanel();
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
