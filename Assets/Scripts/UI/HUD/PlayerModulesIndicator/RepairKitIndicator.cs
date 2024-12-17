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

    int _maxRepairKitNumberForRepair;
    bool _enableStatus;
    event System.Action DisposeEvents;

    public int MaxRepairKitNumberForRepair
    {
        get => _maxRepairKitNumberForRepair;
        set
        {
            _maxRepairKitNumberForRepair = value;

            _enableStatus = !(_maxRepairKitNumberForRepair <= 1);
        }
    }

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(GlobalEventsManager globalEventsManager)
    {
        globalEventsManager.SubscribeToRoundStarted(RoundStart);
        globalEventsManager.SubscribeToRoundEnded(RoundEnd);

        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);
    }

    private void Start()
    {
        RoundEnd();
    }

    public void Initialize(int repairKitNumberForRepair, int currentRepairKit = 0)
    {
        MaxRepairKitNumberForRepair = repairKitNumberForRepair;

        UpdateCurrentRepairKit(currentRepairKit);
    }

    private void RoundStart()
    {
        SetDefaultIndicatorPosition();

        gameObject.SetActive(_enableStatus);
    }

    private void RoundEnd()
    {
        gameObject.SetActive(false);
    }

    public void SetNewIndicatorPosition(Vector2 position)
    {
        _iconTransform.position = position;
    }

    public void SetDefaultIndicatorPosition()
    {
        _iconTransform.position = _defaultIconPosition.position;
    }


    public void UpdateCurrentRepairKit(int currentRepairKit)
    {
        if (MaxRepairKitNumberForRepair <= 1) return;

        _updateCurrentRepairKit.Play();

        currentRepairKit = Mathf.Clamp(currentRepairKit, 0, MaxRepairKitNumberForRepair);
        _frontRepairKit.fillAmount = (float)currentRepairKit / MaxRepairKitNumberForRepair;
    }

    private void OnDestroy()
    {
        DisposeEvents?.Invoke();
    }
}

