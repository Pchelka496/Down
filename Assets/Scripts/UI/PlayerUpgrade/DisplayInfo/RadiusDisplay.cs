using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class RadiusDisplay : MonoBehaviour, IDisplayInfo
{
    [SerializeField] DisplayObject _currentRadius;
    [SerializeField] DisplayObject _targetRadius;

    [SerializeField] float _flashInterval = 0.1f;
    [SerializeField] float _animationDuration = 0.6f;
    [SerializeField] Ease _growthEase = Ease.OutBack;

    RectTransform _playerPosition;

    CancellationTokenSource _animationCts;

    public void Initialize(RectTransform playerViewUpgradePosition)
    {
        _playerPosition = playerViewUpgradePosition;
    }

    public void DisplayRadius(RadiusDisplayData currentCircleData, RadiusDisplayData targetCircleData)
    {
        CirclePositionSetting(currentCircleData, targetCircleData);
        gameObject.SetActive(true);

        AnimateCircle(_currentRadius, currentCircleData);
        AnimateCircle(_targetRadius, targetCircleData);

        ClearToken();
        _animationCts = new();

        Flashing(_animationCts.Token, _targetRadius.Image, targetCircleData.Color).Forget();
    }

    private void CirclePositionSetting(RadiusDisplayData currentCircleData, RadiusDisplayData targetCircleData)
    {
        if (currentCircleData.GlobalPosition == null)
        {
            _currentRadius.RectTransform.position = _playerPosition.position;
        }
        else
        {
            _currentRadius.RectTransform.position = currentCircleData.GlobalPosition.Value;
        }

        if (targetCircleData.GlobalPosition == null)
        {
            _targetRadius.RectTransform.position = _playerPosition.position;
        }
        else
        {
            _targetRadius.RectTransform.position = targetCircleData.GlobalPosition.Value;
        }
    }

    public void StopDisplaying()
    {
        DOTween.Kill(_currentRadius.Circle);
        DOTween.Kill(_currentRadius.CircleMask);
        DOTween.Kill(_targetRadius.Circle);
        DOTween.Kill(_targetRadius.CircleMask);

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
        if (image == null || image.gameObject == null)
        {
            Debug.LogWarning("Trying to animate a destroyed object");
            return;
        }

        var startAlpha = image.color.a;
        var elapsed = 0f;

        try
        {
            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);

                if (image != null)
                {
                    var color = image.color;
                    color.a = newAlpha;
                    image.color = color;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            if (image != null)
            {
                var color = image.color;
                color.a = targetAlpha;
                image.color = color;
            }
        }
        catch (OperationCanceledException)
        {
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
        var canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("Canvas is no found!");
            return 0f;
        }

        var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        var worldPosition = _playerPosition.position + Vector3.right * worldRadius;

        var screenCenter = RectTransformUtility.WorldToScreenPoint(camera, _playerPosition.position);
        var screenEdge = RectTransformUtility.WorldToScreenPoint(camera, worldPosition);

        return Vector2.Distance(screenCenter, screenEdge);
    }

    private void ShrinkCircle(DisplayObject displayObject)
    {
        DOTween.Kill(displayObject.Circle);
        DOTween.Kill(displayObject.CircleMask);

        displayObject.Circle.DOScale(0f, _animationDuration).SetEase(Ease.InBack).OnComplete(() =>
        {
            if (gameObject != null)
            {
                gameObject.SetActive(false);
            }
        });

        displayObject.CircleMask.DOScale(0f, _animationDuration).SetEase(Ease.InBack);
    }

    private void ClearToken() => ClearTokenSupport.ClearToken(ref _animationCts);

    private void OnDestroy()
    {
        StopDisplaying();
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
        public Vector2? GlobalPosition;//If null, the circles will be at the character's position
        public float Radius;
        public float Thickness;
        public Color Color;
    }

}
