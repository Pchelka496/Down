using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;

public class ScreenTouchController : OnScreenButton, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] TouchHandler _leftTouch;
    [SerializeField] TouchHandler _rightTouch;

    [SerializeField] TouchHandler _rightSwipe;
    [SerializeField] TouchHandler _leftSwipe;
    [SerializeField] TouchHandler _upSwipe;
    [SerializeField] TouchHandler _downSwipe;

    [SerializeField] float minSwipeDistance = 50f;

    Vector2 _touchStartPosition;

    public new void OnPointerDown(PointerEventData eventData)
    {
        _touchStartPosition = eventData.position;

        if (eventData.position.x < Screen.width / 2)
        {
            _leftTouch.OnPointerDown(eventData);
        }
        else
        {
            _rightTouch.OnPointerDown(eventData);
        }
    }

    public new void OnPointerUp(PointerEventData eventData)
    {
        _leftTouch.OnPointerUp(eventData);
        _rightTouch.OnPointerUp(eventData);

        ActivateSwipe(eventData);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ActivateSwipe(PointerEventData eventData)
    {
        Vector2 endPosition = eventData.position;
        Vector2 swipeDelta = endPosition - _touchStartPosition;

        if (swipeDelta.magnitude >= minSwipeDistance)
        {
            if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
            {
                if (swipeDelta.x > 0)
                {
                    _rightSwipe.OnPointerDown(eventData);
                    _rightSwipe.OnPointerUp(eventData);
                }
                else
                {
                    _leftSwipe.OnPointerDown(eventData);
                    _leftSwipe.OnPointerUp(eventData);
                }
            }
            else
            {
                if (swipeDelta.y > 0)
                {
                    _upSwipe.OnPointerDown(eventData);
                    _upSwipe.OnPointerUp(eventData);
                }
                else
                {
                    _downSwipe.OnPointerDown(eventData);
                    _downSwipe.OnPointerUp(eventData);
                }
            }
        }
    }

}


