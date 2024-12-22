using TMPro;
using Types.record;
using UnityEngine;
using UnityEngine.UI;

public class UnlockSkinButtonController : MonoBehaviour
{
    [SerializeField] Button _applyButton;
    [SerializeField] Button _buyForFreeButton;
    [SerializeField] Button _diamondsBuyButton;
    [SerializeField] Button _moneyBuyButton;

    [SerializeField] TextMeshProUGUI _applyButtonText;
    [SerializeField] TextMeshProUGUI _buyForFreeButtonText;
    [Header("For the price")]
    [SerializeField] TextMeshProUGUI _diamondsBuyButtonText;
    [SerializeField] TextMeshProUGUI _moneyBuyButtonText;

    [SerializeField] TextContainer _buyForFreeButtonTextContainer;
    [SerializeField] TextContainer _applyButtonTextContainer;

    Button[] _allButton;

    ILanguageContainer _languageContainer;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(ILanguageContainer languageContainer)
    {
        _languageContainer = languageContainer;
        _allButton = new Button[] { _applyButton, _buyForFreeButton, _diamondsBuyButton, _moneyBuyButton };
    }

    public void Initialize(UnityEngine.Events.UnityAction onUnlockButtonClick, UnityEngine.Events.UnityAction onApplyButtonClick)
    {
        _applyButton.onClick.AddListener(onApplyButtonClick);
        _diamondsBuyButton.onClick.AddListener(onUnlockButtonClick);
        _moneyBuyButton.onClick.AddListener(onUnlockButtonClick);
        _buyForFreeButton.onClick.AddListener(onUnlockButtonClick);
    }

    public void UpdateSkinData(PlayerSkinData skinData)
    {
        if (skinData == null) return;

        if (skinData.IsUnlocked)
        {
            _applyButtonText.text = _applyButtonTextContainer.GetText(_languageContainer.Language);
            _applyButton.gameObject.SetActive(true);

            DisableAllButtonExcept(_applyButton);

            return;
        }

        switch (skinData.UnlockMethod)
        {
            case PlayerSkinData.EnumUnlockMethod.Free:
                {
                    _buyForFreeButtonText.text = _buyForFreeButtonTextContainer.GetText(_languageContainer.Language);
                    _buyForFreeButton.gameObject.SetActive(true);

                    DisableAllButtonExcept(exceptButton: _buyForFreeButton);

                    break;
                }
            case PlayerSkinData.EnumUnlockMethod.BuyForMoney:
                {
                    _moneyBuyButton.gameObject.SetActive(true);
                    _moneyBuyButtonText.text = skinData.Cost.ToString();

                    DisableAllButtonExcept(exceptButton: _moneyBuyButton);

                    break;
                }
            case PlayerSkinData.EnumUnlockMethod.BuyForDiamonds:
                {
                    _diamondsBuyButton.gameObject.SetActive(true);
                    _diamondsBuyButtonText.text = skinData.Cost.ToString();

                    DisableAllButtonExcept(exceptButton: _diamondsBuyButton);

                    break;
                }
            default:
                {
                    Debug.LogError($"Unknown enum UnlockMethod - {skinData.UnlockMethod}. {GetType()}");
                    break;
                }
        }
    }

    private void DisableAllButtonExcept(Button exceptButton)
    {
        foreach (var button in _allButton)
        {
            if (button == exceptButton) continue;

            button.gameObject.SetActive(false);
        }
    }
}
