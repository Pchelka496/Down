using Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class RepairKitIndicator : MonoBehaviour
{
    [Header("Front image is filled image type")]
    [SerializeField] Animation _updateCurrentRepairKit;
    [SerializeField] RectTransform _defaultIconPosition;
    [SerializeField] RectTransform _iconTransform;
    [SerializeField] Image _frontRepairKit;
    [SerializeField] Image _backRepairKit;
    [SerializeField] Material _defaultMaterial;
    [SerializeField] Material _flashMaterial;

    int _maxRepairKitNumberForRepair;
    bool _enableStatus;

    public int MaxRepairKitNumberForRepair
    {
        get => _maxRepairKitNumberForRepair; set
        {
            _maxRepairKitNumberForRepair = value;

            if (_maxRepairKitNumberForRepair <= 1)
            {
                _enableStatus = false;
            }
            else
            {
                _enableStatus = true;
            }
        }
    }

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:������� �������������� �������� �����", Justification = "<��������>")]
    private void Construct(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
        gameObject.SetActive(false);
    }

    public void Initialize(int repairKitNumberForRepair, int currentRepairKit = 0)
    {
        MaxRepairKitNumberForRepair = repairKitNumberForRepair;

        UpdateCurrentRepairKit(currentRepairKit);
    }

    public void SetNewIndicatorPosition(Vector2 position)
    {
        _iconTransform.position = position;
    }

    public void SetDefaultIndicatorPosition()
    {
        _iconTransform.position = _defaultIconPosition.position;
    }

    private void RoundStart(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundEnd(RoundEnd);
        SetDefaultIndicatorPosition();

        if (_enableStatus)
        {
            gameObject.SetActive(true);
        }
    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
        gameObject.SetActive(false);
    }

    public void UpdateCurrentRepairKit(int currentRepairKit)
    {
        if (MaxRepairKitNumberForRepair <= 1) return;

        _updateCurrentRepairKit.Play();

        currentRepairKit = Mathf.Clamp(currentRepairKit, 0, MaxRepairKitNumberForRepair);
        _frontRepairKit.fillAmount = (float)currentRepairKit / MaxRepairKitNumberForRepair;
    }

    //Animation event
    public void SetFlashMaterial()
    {
        //_frontRepairKit.material = _flashMaterial;
        //_backRepairKit.material = _flashMaterial;
    }

    public void SetDefaultMaterial()
    {
        //_frontRepairKit.material = _defaultMaterial;
        //_backRepairKit.material = _defaultMaterial;
    }

}

