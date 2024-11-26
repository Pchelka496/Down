using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using System.Threading;

public class TextDisplay : MonoBehaviour, IDisplayInfo
{
    [SerializeField] TextMeshProUGUI _text;

    [SerializeField] float _typingDelay = 0.05f;
    CancellationTokenSource _cts;

    public async UniTask DisplayText(string text)
    {
        StopDisplaying();

        _cts = new();
        var token = _cts.Token;

        _text.text = string.Empty;

        foreach (char character in text)
        {
            _text.text += character;
            await UniTask.Delay((int)(_typingDelay * 1000), cancellationToken: token);
        }

    }

    public void StopDisplaying()
    {
        ClearToken();

        _text.text = string.Empty;
    }

    private void ClearToken() => ClearTokenSupport.ClearToken(ref _cts);

    private void OnDestroy()
    {
        ClearToken();
    }

}

