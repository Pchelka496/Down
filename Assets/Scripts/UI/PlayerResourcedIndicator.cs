using Additional;
using System.Threading;
using TMPro;
using UnityEngine;

public class PlayerResourcedIndicator : MonoBehaviour
{
    const int FAST_UPDATE_TEXT_THRESHOLD = 99;
    const float FAST_UPDATE_TEXT_DELAY = 0.02f;

    [SerializeField] float _updateTextTime = 1f;
    [SerializeField] AnimationCurve _updateTextCurve;
    [SerializeField] TextIndicatorData _money;
    [SerializeField] TextIndicatorData _diamond;
    [SerializeField] TextIndicatorData _energy;

    public void UpdateMoneyText(int moneyValue) => UpdateText(_money, moneyValue);
    public void UpdateDiamondText(int diamondValue) => UpdateText(_diamond, diamondValue);
    public void UpdateEnergyText(int energyValue) => UpdateText(_energy, energyValue);


    private void UpdateText(TextIndicatorData textIndicatorData, int newValue)
    {
        textIndicatorData.ClearToken();

        var cts = new CancellationTokenSource();
        textIndicatorData.Cts = cts;


        if (int.TryParse(textIndicatorData.TextTpm.text, out var currentValue))
        {
            int difference = Mathf.Abs(currentValue - newValue);

            if (difference < FAST_UPDATE_TEXT_THRESHOLD)
            {
                textIndicatorData.TextTpm.SmoothUpdateText(newValue, cts.Token, FAST_UPDATE_TEXT_DELAY).Forget();
                return;
            }
        }

        textIndicatorData.TextTpm.SmoothUpdateTextWithDuration(newValue, cts.Token, _updateTextTime, _updateTextCurve).Forget();
    }

    private void OnDestroy()
    {
        _money.Dispose();
        _diamond.Dispose();
        _energy.Dispose();
    }

    [System.Serializable]
    private record TextIndicatorData : System.IDisposable
    {
        [SerializeField] TextMeshProUGUI _textTpm;
        CancellationTokenSource _cts;

        public TextMeshProUGUI TextTpm => _textTpm;
        public CancellationTokenSource Cts { set => _cts = value; }

        public void ClearToken()
        {
            ClearTokenSupport.ClearToken(ref _cts);
        }

        public void Dispose()
        {
            ClearToken();
        }
    }
}
