using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
public class ScrollRectController
{
    [SerializeField] ScrollRect _scrollRect;
    [SerializeField] RectTransform _targetPosition;
    [SerializeField] RectTransform _content;

    [SerializeField] float _animationDuration = 0.5f;
    [SerializeField] Ease _animationEase = Ease.OutCubic;

    public void ViewDetailedInformation(RectTransform targetObject)
    {
        _scrollRect.enabled = false;

        var contentPositionY = CalculateTargetContentPositionY(targetObject);

        _content.DOLocalMoveY(contentPositionY, _animationDuration)
                .SetEase(_animationEase);
    }

    public void DefaultWork()
    {
        _scrollRect.enabled = true;
    }

    private float CalculateTargetContentPositionY(RectTransform targetObject)
    {
        Vector3 targetLocalPosition = _content.InverseTransformPoint(targetObject.position);

        Vector3 targetDesiredPosition = _content.InverseTransformPoint(_targetPosition.position);

        float differenceY = targetLocalPosition.y - targetDesiredPosition.y;

        return _content.localPosition.y - differenceY;
    }

}

