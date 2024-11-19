using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class RadiusDisplay : MonoBehaviour, IDisplayInfo
{
    [SerializeField] DisplayObject _currentRadius;
    [SerializeField] DisplayObject _targetRadius;

    [SerializeField] float _flashInterval = 0.1f;
    [SerializeField] float _animationDuration = 0.6f;
    [SerializeField] Ease _growthEase = Ease.OutBack;

    RectTransform _playerPosition;

    CancellationTokenSource _flashCts;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Initialize(RectTransform playerViewUpgradePosition)
    {
        _playerPosition = playerViewUpgradePosition;

        _currentRadius.RectTransform.position = playerViewUpgradePosition.position;
        _targetRadius.RectTransform.position = playerViewUpgradePosition.position;
    }

    public void DisplayRadius(RadiusDisplayData currentCircleData, RadiusDisplayData targetCircleData)
    {
        gameObject.SetActive(true);

        AnimateCircle(_currentRadius, currentCircleData);
        AnimateCircle(_targetRadius, targetCircleData);

        ClearToken();
        _flashCts = new();

        Flashing(_flashCts.Token, _targetRadius.Image, targetCircleData.Color).Forget();
    }

    public void StopDisplaying()
    {
        gameObject.SetActive(false);

       
    }

    private async UniTask Flashing(CancellationToken token, Image image, Color defaultColor)
    {
        while (!token.IsCancellationRequested)
        {
            image.color = defaultColor;

            await AnimateFade(image, 0f, _flashInterval, token);

            await AnimateFade(image, defaultColor.a, _flashInterval, token);
        }
    }

    private async UniTask AnimateFade(Image image, float targetAlpha, float duration, CancellationToken token)
    {
        var tcs = new UniTaskCompletionSource();

        image.DOFade(targetAlpha, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() => tcs.TrySetResult());

        using (token.Register(() => tcs.TrySetCanceled()))
        {
            await tcs.Task;
        }
    }

    public void HideRadius()
    {
        ShrinkCircle(_currentRadius);
        ShrinkCircle(_targetRadius);

        ClearToken();
    }

    private void AnimateCircle(DisplayObject displayObject, RadiusDisplayData data)
    {
        float radiusInPixels = WorldRadiusToCanvasRadius(data.Radius);

        var sizeDelta = new Vector2(radiusInPixels * 2, radiusInPixels * 2);

        displayObject.Circle.localScale = Vector3.zero;
        displayObject.Circle.sizeDelta = sizeDelta;
        displayObject.Image.color = data.Color;

        displayObject.CircleMask.localScale = Vector3.zero;
        displayObject.CircleMask.sizeDelta = new Vector2(sizeDelta.x - data.Thickness, sizeDelta.y - data.Thickness);

        displayObject.Circle.DOScale(1f, _animationDuration).SetEase(_growthEase);
        displayObject.CircleMask.DOScale(1f, _animationDuration).SetEase(_growthEase);
    }

    private float WorldRadiusToCanvasRadius(float worldRadius)
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas не найден!");
            return 0f;
        }

        Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        Vector3 worldPosition = _playerPosition.position + Vector3.right * worldRadius;

        Vector2 screenCenter = RectTransformUtility.WorldToScreenPoint(camera, _playerPosition.position);
        Vector2 screenEdge = RectTransformUtility.WorldToScreenPoint(camera, worldPosition);

        return Vector2.Distance(screenCenter, screenEdge);
    }

    private void ShrinkCircle(DisplayObject displayObject)
    {
        displayObject.Circle.DOScale(0f, _animationDuration).SetEase(Ease.InBack).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    private void ClearToken() => ClearTokenSupport.ClearToken(ref _flashCts);

    private void OnDestroy()
    {
        ClearToken();
    }

    [System.Serializable]
    private record DisplayObject
    {
        [SerializeField] RectTransform _rectTransform;
        [SerializeField] RectTransform _circleMaskRectTransform;
        [SerializeField] RectTransform _circleRectTransform;
        [SerializeField] Image _image;

        public RectTransform RectTransform => _rectTransform;
        public RectTransform Circle => _circleRectTransform;
        public RectTransform CircleMask => _circleMaskRectTransform;
        public Image Image => _image;

    }

    [System.Serializable]
    public struct RadiusDisplayData
    {
        public float Radius;
        public float Thickness;
        public Color Color;
    }

}
