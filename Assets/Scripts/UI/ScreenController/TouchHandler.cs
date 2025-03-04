using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;

[System.Serializable]
public class TouchHandler : OnScreenButton, IPointerDownHandler, IPointerUpHandler
{
    public new void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
    }

    public new void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
    }

}
