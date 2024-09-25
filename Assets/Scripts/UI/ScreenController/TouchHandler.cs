using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;

public class TouchHandler : OnScreenButton, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
    }

}
