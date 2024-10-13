using UnityEngine;
using Zenject;

public class ScreenTouchController : MonoBehaviour
{
    //[SerializeField] RectTransform[] _touchHandlerTransforms;

    //[SerializeField] float _originalStartTop;
    //[SerializeField] float _roundStartTop;

    //[Inject]
    //private void Construct(LevelManager levelManager)
    //{
    //    levelManager.SubscribeToRoundStart(RoundStart);
    //}

    //private void Start()
    //{
    //    if (_touchHandlerTransforms == null) return;

    //    foreach (var handler in _touchHandlerTransforms)
    //    {
    //        SetTopOffset(handler, _originalStartTop);
    //    }
    //}

    //private void RoundStart()
    //{
    //    if (_touchHandlerTransforms == null) return;

    //    foreach (var handler in _touchHandlerTransforms)
    //    {
    //        SetTopOffset(handler, _roundStartTop);
    //    }
    //}

    //private void SetTopOffset(RectTransform rectTransform, float topOffset)
    //{
    //    float bottomOffset = rectTransform.offsetMin.y;

    //    rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, -topOffset);

    //    rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, bottomOffset);
    //}

}

