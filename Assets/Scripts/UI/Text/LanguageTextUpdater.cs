using Additional;
using System.Threading;
using TMPro;
using UnityEngine;

public class LanguageTextUpdater : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _textMeshPro;
    [SerializeField] TextContainer _textContainer;

    CancellationTokenSource _updateTextCts;
    ILanguageContainer _languageContainer;

    [Zenject.Inject]
    public void Construct(ILanguageContainer languageContainer)
    {
        _languageContainer = languageContainer;
        _languageContainer.SubscribeToChangeLanguageEvent(UpdateText);
    }

    private void Start()
    {
        _textMeshPro.text = _textContainer.GetText(_languageContainer.Language);
    }

    private void UpdateText(EnumLanguage language)
    {
        var newText = _textContainer.GetText(language);

        if (_textMeshPro != null)
        {
            if (isActiveAndEnabled)
            {
                ClearToken();
                _updateTextCts = new();

                const float TEXT_UPDATE_DALEY = 0.02f;
                _textMeshPro.SmoothUpdateText(newText, _updateTextCts.Token, TEXT_UPDATE_DALEY).Forget();
            }
            else
            {
                _textMeshPro.text = newText;
            }
        }
        else
        {
            Debug.LogError("TextMeshProUGUI is not assigned.");
        }
    }

    private void ClearToken() => ClearTokenSupport.ClearToken(ref _updateTextCts);

    private void OnDestroy()
    {
        _languageContainer.UnsubscribeFromChangeLanguageEvent(UpdateText);
        ClearToken();
    }

    private void Reset()
    {
        _textMeshPro = gameObject.GetComponent<TextMeshProUGUI>();
    }
}
