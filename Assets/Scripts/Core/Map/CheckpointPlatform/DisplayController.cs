using TMPro;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DisplayController : MonoBehaviour
{
    const char CURSOR_CHAR = '|';
    const float TYPING_SPEED = 0.1f;
    const float ERASE_SPEED = 0.05f;
    const float CURSOR_BLINK_SPEED = 0.5f;

    [SerializeField] TextMeshProUGUI _displayText;
    [SerializeField] TextConfigLanguageAddresses _textConfigLanguageAddresses;
    PointKeeper _pointKeeper;
    DisplayControllerConfig _config;
    string _targetText;
    CancellationTokenSource _cts;
    bool _cursorVisible = true;

    [Inject]
    private async void Construct(EnumLanguage language, PointKeeper pointKeeper)
    {
        switch (language)
        {
            case EnumLanguage.English:
                {
                    _config = await LoadConfig(_textConfigLanguageAddresses.EnglishTextConfigAddress);
                    break;
                }
            case EnumLanguage.Russian:
                {
                    _config = await LoadConfig(_textConfigLanguageAddresses.RussianTextConfigAddress);
                    break;
                }
            case EnumLanguage.Ukrainian:
                {
                    _config = await LoadConfig(_textConfigLanguageAddresses.UkrainianTextConfigAddress);
                    break;
                }
            case EnumLanguage.Chinese:
                {
                    _config = await LoadConfig(_textConfigLanguageAddresses.ChineseTextConfigAddress);
                    break;
                }
            default:
                {
                    Construct(EnumLanguage.English, pointKeeper);
                    return;
                }
        }

        _pointKeeper = pointKeeper;
        TargetText(DefaultText()).Forget();
    }

    public async UniTask SetClimbingMode(float displayTime)
    {
        await TargetText(_config.SetClimbingMode, displayTime);
    }

    public async UniTask SetDescendingMode(float displayTime)
    {
        await TargetText(_config.SetDescendingMode, displayTime);
    }

    private async UniTask TargetText(string value, float displayTime = 0f)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        _targetText = value;

        await StartTypingTextAsync(_targetText, displayTime);
    }

    private string ConvertText(string startText, int value, string endText)
    {
        return $"{startText}\n{value:N0}{endText}";
    }

    private async UniTask<DisplayControllerConfig> LoadConfig(string address)
    {
        AsyncOperationHandle<DisplayControllerConfig> handle = Addressables.LoadAssetAsync<DisplayControllerConfig>(address);

        await handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }
        else
        {
            Debug.LogError("Error loading via Addressables.");
            return default;
        }
    }

    private async UniTask StartTypingTextAsync(string targetText, float displayTime)
    {
        // Очищаем текст сразу и стираем, если что-то есть
        await EraseTextAsync(_cts.Token);

        // Убеждаемся, что поле очищено перед печатью
        _displayText.text = string.Empty;

        await TypeTextAsync(targetText, _cts.Token, displayTime);
    }

    private async UniTask EraseTextAsync(CancellationToken token)
    {
        while (_displayText.text.Length > 0)
        {
            if (token.IsCancellationRequested) return;

            if (_displayText.text.Length == 1)
            {
                _displayText.text = string.Empty; 
            }
            else
            {
                _displayText.text = _displayText.text.Substring(0, _displayText.text.Length - 2) + CURSOR_CHAR;// 2 = 1 element + 1 cursor
            }

            await UniTask.WaitForSeconds(ERASE_SPEED);
        }
    }

    private async UniTask TypeTextAsync(string text, CancellationToken token, float displayTime)
    {
        for (int i = 0; i <= text.Length; i++)
        {
            if (token.IsCancellationRequested)
            {
                // Если операция отменена, сразу выходим
                return;
            }

            // Печатаем текст символ за символом с курсором в конце
            _displayText.text = text.Substring(0, i) + CURSOR_CHAR;
            await UniTask.WaitForSeconds(TYPING_SPEED);
        }

        // Начинаем мигание курсора после вывода текста
        StartCursorBlinkingAsync(token).Forget();

        // Если текст не является текстом по умолчанию, ждём указанное время и возвращаемся к тексту по умолчанию
        if (_targetText != DefaultText())
        {
            await UniTask.WaitForSeconds(displayTime);
            TargetText(DefaultText()).Forget();
        }
    }


    private string DefaultText()
    {
        return ConvertText(_config.StartPointsInformation, _pointKeeper.GetPoint(), _config.EndPointsInformation);
    }

    private async UniTask StartCursorBlinkingAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            _cursorVisible = !_cursorVisible;
            _displayText.text = _targetText + (_cursorVisible ? CURSOR_CHAR.ToString() : "");
            await UniTask.WaitForSeconds(CURSOR_BLINK_SPEED);
        }
    }

    [System.Serializable]
    private record TextConfigLanguageAddresses
    {
        [SerializeField] string _russianTextConfigAddress;
        [SerializeField] string _englishTextConfigAddress;
        [SerializeField] string _ukrainianTextConfigAddress;
        [SerializeField] string _chineseTextConfigAddress;

        public string RussianTextConfigAddress { get => _russianTextConfigAddress; }
        public string EnglishTextConfigAddress { get => _englishTextConfigAddress; }
        public string UkrainianTextConfigAddress { get => _ukrainianTextConfigAddress; }
        public string ChineseTextConfigAddress { get => _chineseTextConfigAddress; }

    }

}
