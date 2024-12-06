using Cysharp.Threading.Tasks;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;

public static class SmoothTransitionToText
{
    /// <summary>
    /// Smoothly updates the TextMeshProUGUI text to the target text by adding or removing characters one by one.
    /// </summary>
    /// <param name=“textMeshPro”>TextMeshProUGUI object to update.</param>
    /// <param name=“targetText”>Target text.</param>
    /// <param name=“token”>Cancellation token for process control.</param>
    /// <param name=“textUpdateDelay”>Delay between text changes.</param>
    /// <param name=“suffix”>Transition string added at the end of the text (e.g., “%”).</param>
    public static async UniTaskVoid SmoothUpdateText(
    this TextMeshProUGUI textMeshPro,
    string targetText,
    CancellationToken token,
    float textUpdateDelay,
    string suffix = null)
    {
        if (textMeshPro == null)
        {
            Debug.LogError("TextMeshProUGUI is null!");
            return;
        }

        targetText ??= string.Empty;
        suffix ??= string.Empty;

        var delaySpan = System.TimeSpan.FromSeconds(textUpdateDelay);
        var currentText = new StringBuilder(textMeshPro.text);

        if (currentText.ToString().EndsWith(suffix))
        {
            currentText.Length -= suffix.Length;
        }

        var targetTextWithoutSuffix = new StringBuilder(targetText);

        while (!token.IsCancellationRequested)
        {
            if (currentText.ToString() == targetTextWithoutSuffix.ToString())
                break;

            if (currentText.Length > targetTextWithoutSuffix.Length)
            {
                currentText.Length -= 1;
            }
            else if (currentText.Length < targetTextWithoutSuffix.Length)
            {
                currentText.Append(targetTextWithoutSuffix[currentText.Length]);
            }
            else
            {
                for (int i = 0; i < currentText.Length; i++)
                {
                    if (currentText[i] != targetTextWithoutSuffix[i])
                    {
                        currentText[i] = targetTextWithoutSuffix[i];
                        await UniTask.Delay(delaySpan, cancellationToken: token);
                        break;
                    }
                }
            }

            textMeshPro.text = currentText + suffix;

            await UniTask.Delay(delaySpan, cancellationToken: token);
        }

        textMeshPro.text = targetTextWithoutSuffix + suffix;
    }

    /// <summary>
    /// Smoothly updates the TextMeshProUGUI text value to the target number, incrementing or decrementing by 1 at each step.
    /// </summary>
    /// <param name="textMeshPro">TextMeshProUGUI object to update.</param>
    /// <param name="targetValue">The target numeric value that should be displayed.</param>
    /// <param name="token">Cancellation token to control the update process and cancel it if needed.</param>
    /// <param name="textUpdateDelay">Delay (in seconds) between each value update during the transition.</param>
    /// <param name="suffix">Optional string added at the end of the number (e.g., "%", " coins").</param>
    /// <param name="onValueUpdate">Action that will be called each time the value is updated during the transition. This can be used to trigger additional logic.</param>
    public static async UniTaskVoid SmoothUpdateText(
        this TextMeshProUGUI textMeshPro,
        int targetValue,
        CancellationToken token,
        float textUpdateDelay,
        string suffix = null,
        System.Action<int> onValueUpdate = null
        )
    {
        if (textMeshPro == null)
        {
            Debug.LogError("TextMeshProUGUI is null!");
            return;
        }

        suffix ??= string.Empty;

        string currentText = textMeshPro.text;

        if (!string.IsNullOrEmpty(suffix))
        {
            currentText = currentText.Replace(suffix, string.Empty);
        }

        if (!int.TryParse(currentText, out var currentValue))
        {
            currentValue = 0;
        }

        var delaySpan = System.TimeSpan.FromSeconds(textUpdateDelay);

        while (!token.IsCancellationRequested)
        {
            if (currentValue == targetValue)
                break;

            currentValue += currentValue < targetValue ? 1 : -1;

            textMeshPro.text = currentValue + suffix;
            onValueUpdate?.Invoke(currentValue);

            await UniTask.Delay(delaySpan, cancellationToken: token);
        }

        textMeshPro.text = targetValue + suffix;
        onValueUpdate?.Invoke(targetValue);
    }

    /// <summary>
    /// Smoothly updates the TextMeshProUGUI text value to the target number using a smooth animation curve to interpolate the values.
    /// </summary>
    /// <param name="textMeshPro">TextMeshProUGUI object to update.</param>
    /// <param name="targetValue">The target numeric value that should be displayed at the end of the transition.</param>
    /// <param name="token">Cancellation token to control the update process and cancel it if needed.</param>
    /// <param name="totalDuration">Total time (in seconds) for the update to transition from the current value to the target value.</param>
    /// <param name="curve">Animation curve that defines the easing (smoothness) of the transition (e.g., linear, ease-in, ease-out).</param>
    /// <param name="suffix">Optional string added at the end of the number (e.g., "%", " coins").</param>
    /// <param name="onValueUpdate">Action that will be called each time the value is updated during the transition. This can be used to trigger additional logic during the animation.</param>
    public static async UniTaskVoid SmoothUpdateTextWithDuration(
        this TextMeshProUGUI textMeshPro,
        int targetValue,
        CancellationToken token,
        float totalDuration,
        AnimationCurve curve,
        string suffix = null,
        System.Action<int> onValueUpdate = null
        )
    {
        if (textMeshPro == null)
        {
            Debug.LogError("TextMeshProUGUI is null!");
            return;
        }

        suffix ??= string.Empty;

        string currentText = textMeshPro.text;

        if (!string.IsNullOrEmpty(suffix))
        {
            currentText = currentText.Replace(suffix, string.Empty);
        }

        if (!int.TryParse(currentText, out var currentValue))
        {
            currentValue = 0;
        }

        float elapsedTime = 0f;
        int startValue = currentValue;
        int deltaValue = targetValue - startValue;

        while (!token.IsCancellationRequested && elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / totalDuration);
            float curveT = curve.Evaluate(t);

            int updatedValue = Mathf.RoundToInt(Mathf.Lerp(startValue, targetValue, curveT));

            textMeshPro.text = updatedValue + suffix;
            onValueUpdate?.Invoke(updatedValue);

            await UniTask.Yield();
        }

        textMeshPro.text = targetValue + suffix;
        onValueUpdate?.Invoke(targetValue);
    }
}



