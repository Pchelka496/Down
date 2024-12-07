using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ExchangeButtonContainer : Button, IPointerDownHandler, IPointerUpHandler
{
    [Header("This action will be added in onClick")]
    [SerializeField] UnityEvent _onClickEvent;

    bool _isButtonPressed;

    public void CheckAndClick()
    {
        if (_isButtonPressed)
        {
            onClick.Invoke();
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        _isButtonPressed = true;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        _isButtonPressed = false;
    }
}
