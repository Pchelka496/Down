using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] Image _revealMask;
    [SerializeField] Image _backgroundOverlay;
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] float _fadeDuration = 1f;
    [SerializeField] AnimationCurve _fadeCurve;
    [SerializeField] Vector2 _startSize = new(0, 0);
    Vector2 _endSize = GetCircleBoundingSquareSize();

    RectTransform _imageRectTransform;

    private void Awake()
    {
        _backgroundOverlay.rectTransform.sizeDelta = _endSize;

        _imageRectTransform = _revealMask.GetComponent<RectTransform>();
        _imageRectTransform.sizeDelta = _startSize;
        _revealMask.gameObject.SetActive(true);
    }

    public async UniTask FadeToClear()
    {
        _canvasGroup.blocksRaycasts = true;
        _revealMask.gameObject.SetActive(true);
        _backgroundOverlay.gameObject.SetActive(true);

        _imageRectTransform.sizeDelta = _startSize;

        var tween = _imageRectTransform.DOSizeDelta(_endSize, _fadeDuration)
            .SetEase(_fadeCurve)
            .OnComplete(() =>
            {
                _revealMask.gameObject.SetActive(false);
                _backgroundOverlay.gameObject.SetActive(false);
            });

        await tween.AsyncWaitForCompletion();
    }

    public async UniTask FadeToBlack()
    {
        _canvasGroup.blocksRaycasts = true;
        _revealMask.gameObject.SetActive(true);
        _backgroundOverlay.gameObject.SetActive(true);

        var tween = _imageRectTransform.DOSizeDelta(_startSize, _fadeDuration)
            .SetEase(_fadeCurve)
            .OnComplete(() =>
            {
                _revealMask.gameObject.SetActive(false);
            });

        await tween.AsyncWaitForCompletion();
    }

    public static Vector2 GetCircleBoundingSquareSize()
    {
        float diagonal = Mathf.Sqrt(Mathf.Pow(Screen.width, 2) + Mathf.Pow(Screen.height, 2));
        return new Vector2(diagonal, diagonal);
    }

}

