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
    public static async UniTaskVoid SmoothUpdateText(this TextMeshProUGUI textMeshPro,
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

            textMeshPro.text = currentText.ToString() + suffix;

            await UniTask.Delay(delaySpan, cancellationToken: token);
        }

        textMeshPro.text = targetTextWithoutSuffix.ToString() + suffix;
    }

    /// <summary>
    /// Smoothly updates the TextMeshProUGUI text value to the target number, incrementing or decrementing by 1 at each step.
    /// </summary>
    public static async UniTask SmoothUpdateText(this TextMeshProUGUI textMeshPro,
                                                 int targetValue,
                                                 CancellationToken token,
                                                 float textUpdateDelay,
                                                 string suffix = null,
                                                 System.Action<int> onValueUpdate = null)
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
        var builder = new StringBuilder();

        while (!token.IsCancellationRequested)
        {
            if (currentValue == targetValue)
                break;

            currentValue += currentValue < targetValue ? 1 : -1;

            builder.Clear();
            builder.Append(currentValue).Append(suffix);
            textMeshPro.text = builder.ToString();

            onValueUpdate?.Invoke(currentValue);
            await UniTask.Delay(delaySpan, cancellationToken: token);
        }

        builder.Clear();
        builder.Append(targetValue).Append(suffix);
        textMeshPro.text = builder.ToString();
        onValueUpdate?.Invoke(targetValue);
    }

    /// <summary>
    /// Smoothly updates the TextMeshProUGUI text value to the target number using a smooth animation curve to interpolate the values.
    /// </summary>
    public static async UniTask SmoothUpdateTextWithDuration(this TextMeshProUGUI textMeshPro,
                                                             int targetValue,
                                                             CancellationToken token,
                                                             float totalDuration,
                                                             AnimationCurve curve = null,
                                                             string suffix = null,
                                                             System.Action<int> onValueUpdate = null)
    {
        if (textMeshPro == null)
        {
            Debug.LogError("TextMeshProUGUI is null!");
            return;
        }

        curve ??= AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
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

        var elapsedTime = 0f;
        var startValue = currentValue;
        var deltaValue = targetValue - startValue;
        var builder = new StringBuilder();

        while (!token.IsCancellationRequested && elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;

            var t = Mathf.Clamp01(elapsedTime / totalDuration);
            var curveT = curve.Evaluate(t);

            int updatedValue = Mathf.RoundToInt(Mathf.Lerp(startValue, targetValue, curveT));

            builder.Clear();
            builder.Append(updatedValue).Append(suffix);
            textMeshPro.text = builder.ToString();

            onValueUpdate?.Invoke(updatedValue);
            await UniTask.Yield();
        }

        builder.Clear();
        builder.Append(targetValue).Append(suffix);
        textMeshPro.text = builder.ToString();
        onValueUpdate?.Invoke(targetValue);
    }
}
