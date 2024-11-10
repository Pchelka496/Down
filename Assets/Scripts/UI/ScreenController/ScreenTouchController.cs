using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using Zenject;

public class ScreenTouchController : OnScreenButton, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    Camera _mainCamera;
    Vector2 _touchStartPosition;
    Vector2 _touchEndPosition;
    Vector2 _currentTouchPosition;

    public Vector2 TouchStartPosition => _touchStartPosition;
    public Vector2 TouchEndPosition => _touchEndPosition;
    public Vector2 TouchCurrentPosition => _currentTouchPosition;

    [Inject]
    private void Construct(Camera mainCamera)
    {
        _mainCamera = mainCamera;
    }

    public new void OnPointerDown(PointerEventData eventData)
    {
        _touchStartPosition = eventData.position;
        OnDrag(eventData);

        base.OnPointerDown(eventData);
    }

    public new void OnPointerUp(PointerEventData eventData)
    {
        _touchEndPosition = eventData.position;
        OnDrag(eventData);

        base.OnPointerUp(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        _currentTouchPosition = eventData.position;
        Debug.DrawLine(_mainCamera.ScreenToWorldPoint(new Vector3(_touchStartPosition.x, _touchStartPosition.y, 10f)),
                      _mainCamera.ScreenToWorldPoint(new Vector3(_currentTouchPosition.x, _currentTouchPosition.y, 10f)),
                      Color.red);
    }

}


