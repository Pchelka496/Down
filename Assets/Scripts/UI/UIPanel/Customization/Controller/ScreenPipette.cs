using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

public class ScreenPipette : OnScreenButton, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] Image _image;

    Texture2D _texture;
    event Action<Color> OnColorPicked;
    event Action<Color> OnColorPickedEnd;

    private void Start()
    {
        if (_image.sprite != null && _image.sprite.texture != null)
        {
            _texture = _image.sprite.texture;
        }
        else
        {
            Debug.LogError("Image must have a valid sprite with a texture!");
        }
    }

    public void SubscribeToOnColorPicked(Action<Color> action) => OnColorPicked += action;
    public void UnsubscribeFromOnColorPicked(Action<Color> action) => OnColorPicked -= action;

    public void SubscribeToOnColorPickedEnd(Action<Color> action) => OnColorPickedEnd += action;
    public void UnsubscribeFromOnColorPickedEnd(Action<Color> action) => OnColorPickedEnd -= action;

    public new void OnPointerDown(PointerEventData eventData)
    {
        OnColorPicked?.Invoke(TryPickColor(eventData));
    }

    public new void OnPointerUp(PointerEventData eventData)
    {
        OnColorPicked?.Invoke(TryPickColor(eventData));
        OnColorPickedEnd?.Invoke(TryPickColor(eventData));
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnColorPicked?.Invoke(TryPickColor(eventData));
    }

    private Color TryPickColor(PointerEventData eventData)
    {
        if (_texture == null) return default;

        RectTransform rectTransform = _image.rectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            Rect rect = rectTransform.rect;

            var u = Mathf.Clamp01((localPoint.x - rect.xMin) / rect.width);
            var v = Mathf.Clamp01((localPoint.y - rect.yMin) / rect.height);

            var x = Mathf.Clamp(Mathf.RoundToInt(u * _texture.width), 0, _texture.width - 1);
            var y = Mathf.Clamp(Mathf.RoundToInt(v * _texture.height), 0, _texture.height - 1);

            Color pickedColor = _texture.GetPixel(x, y);

            return pickedColor;
        }

        return default;
    }

    private void OnDestroy()
    {
        OnColorPicked = null;
    }

}


