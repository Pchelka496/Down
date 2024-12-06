using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class LevelProgressDisplay
{
    public enum FillDirection
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop
    }

    [SerializeField] RectTransform _containerParent;
    [SerializeField] RectTransform _container;
    [SerializeField] Sprite _filledSprite;
    [SerializeField] Sprite _emptySprite;
    [SerializeField] Color _filledColor = Color.green;
    [SerializeField] Color _emptyColor = Color.gray;

    [SerializeField] float _leftPadding = 10f;
    [SerializeField] float _rightPadding = 10f;
    [SerializeField] float _topPadding = 10f;
    [SerializeField] float _bottomPadding = 10f;
    [SerializeField] float _spacing = 5f;
    [SerializeField] FillDirection _direction = FillDirection.LeftToRight;

    Image[] _levelImages;

    public async UniTask Initialize(int maxLevel)
    {
        await UniTask.DelayFrame(1);

        ClearContainer();
        _levelImages = new Image[maxLevel];

        (float elementWidth, float elementHeight) = CalculateElementSize(maxLevel);

        for (int i = 0; i < maxLevel; i++)
        {
            CreateAndPositionElement(i, elementWidth, elementHeight);
        }

        await UniTask.CompletedTask;
    }

    private void ClearContainer()
    {
        foreach (Transform child in _container)
        {
            MonoBehaviour.Destroy(child.gameObject);
        }
    }

    private (float elementWidth, float elementHeight) CalculateElementSize(int maxLevel)
    {
        float containerWidth = _container.rect.width - _leftPadding - _rightPadding;
        float containerHeight = _container.rect.height - _topPadding - _bottomPadding;

        float elementWidth = _direction == FillDirection.LeftToRight || _direction == FillDirection.RightToLeft
            ? (containerWidth - (_spacing * (maxLevel - 1))) / maxLevel
            : containerWidth;

        float elementHeight = _direction == FillDirection.TopToBottom || _direction == FillDirection.BottomToTop
            ? (containerHeight - (_spacing * (maxLevel - 1))) / maxLevel
            : containerHeight;

        return (elementWidth, elementHeight);
    }

    private void CreateAndPositionElement(int index, float elementWidth, float elementHeight)
    {
        var levelObject = new GameObject("Level_" + index, typeof(Image));
        levelObject.transform.SetParent(_container, false);

        RectTransform rectTransform = levelObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(elementWidth, elementHeight);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        SetElementPosition(rectTransform, index, elementWidth, elementHeight);

        Image image = levelObject.GetComponent<Image>();
        image.sprite = _emptySprite;
        image.color = _emptyColor;

        _levelImages[index] = image;
    }

    private void SetElementPosition(RectTransform rectTransform, int index, float elementWidth, float elementHeight)
    {
        switch (_direction)
        {
            case FillDirection.LeftToRight:
                rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0, 0.5f);
                rectTransform.anchoredPosition = new Vector2(
                    _leftPadding + index * (elementWidth + _spacing) + elementWidth / 2,
                    0f
                );
                break;

            case FillDirection.RightToLeft:
                rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(1, 0.5f);
                rectTransform.anchoredPosition = new Vector2(
                    -_rightPadding - index * (elementWidth + _spacing) - elementWidth / 2,
                    0f
                );
                break;

            case FillDirection.TopToBottom:
                rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 1);
                rectTransform.anchoredPosition = new Vector2(
                    0f,
                    -_topPadding - index * (elementHeight + _spacing) - elementHeight / 2
                );
                break;

            case FillDirection.BottomToTop:
                rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0);
                rectTransform.anchoredPosition = new Vector2(
                    0f,
                    _bottomPadding + index * (elementHeight + _spacing) + elementHeight / 2
                );
                break;
        }
    }


    public void UpdateLevelProgress(int activeLevels)
    {
        for (int i = 0; i < _levelImages.Length; i++)
        {
            if (i < activeLevels)
            {
                _levelImages[i].sprite = _filledSprite;
                _levelImages[i].color = _filledColor;
            }
            else
            {
                _levelImages[i].sprite = _emptySprite;
                _levelImages[i].color = _emptyColor;
            }
        }
    }
}

