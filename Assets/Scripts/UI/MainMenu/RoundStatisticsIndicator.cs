using Nenn.InspectorEnhancements.Runtime.Attributes;
using TMPro;
using UnityEngine;

public class RoundStatisticsIndicator : MonoBehaviour
{
    [HideLabel][Required][SerializeField] TextMeshProUGUI _textMeshPro;
    [SerializeField] TextContainer _totalMoneyCollectedText;
    [SerializeField] TextContainer _totalCollisionsText;
    [SerializeField] RoundResultText _resultText;
    [SerializeField] TextContainer _roundDurationText;

    EnumLanguage _language;

    event System.Action DisposeEvents;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(GameAnalytics gameAnalytics, ILanguageContainer languageContainer)
    {
        _language = languageContainer.Language;

        gameAnalytics.SubscribeToOnAnalyticsUpdated(OnAnaliticsUpdated);
        DisposeEvents += () => gameAnalytics?.UnsubscribeFromOnAnalyticsUpdated(OnAnaliticsUpdated);

        languageContainer.SubscribeToChangeLanguageEvent(OnLanguageChange);
        DisposeEvents += () => languageContainer?.UnsubscribeFromChangeLanguageEvent(OnLanguageChange);
    }

    private void OnAnaliticsUpdated(AnalyticsData roundData)
    {
        UpdateText(roundData);
    }

    private void UpdateText(AnalyticsData currentRoundData)
    {
        var totalMoneyCollectedText = $"{_totalMoneyCollectedText.GetText(_language)} {currentRoundData.TotalMoneyCollected}";
        var totalCollisionsText = $"{_totalCollisionsText.GetText(_language)} {currentRoundData.TotalCollisions}";
        var roundDurationText = $"{_roundDurationText.GetText(_language)} {currentRoundData.RoundDuration:F1}s";
        var resultText = $"{_resultText.GetText(currentRoundData.Result, _language)}";

        var text = $"{totalMoneyCollectedText}\n{totalCollisionsText}\n{roundDurationText}\n{resultText}";

        _textMeshPro.text = text;
    }

    private void OnLanguageChange(EnumLanguage language)
    {
        _language = language;
    }

    private void OnDestroy()
    {
        DisposeEvents?.Invoke();
    }

    [System.Serializable]
    private record RoundResultText
    {
        [SerializeField] TextContainer _unfinishedText;
        [SerializeField] TextContainer _victoryText;
        [SerializeField] TextContainer _defeatText;

        public string GetText(EnumRoundResult roundResult, EnumLanguage language)
        {
            return roundResult switch
            {
                EnumRoundResult.Unfinished => _unfinishedText.GetText(language),
                EnumRoundResult.Victory => _victoryText.GetText(language),
                EnumRoundResult.Defeat => _defeatText.GetText(language),

                _ => throw new System.NotImplementedException(),
            };
        }
    }
}
