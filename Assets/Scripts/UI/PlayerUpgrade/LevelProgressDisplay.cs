using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class LevelProgressDisplay
{
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

    Image[] _levelImages;

    public async UniTask Initialize(int currentLevel, int maxLevel)
    {
        await UniTask.DelayFrame(1);

        foreach (Transform child in _container)
        {
            MonoBehaviour.Destroy(child.gameObject);
        }

        _levelImages = new Image[maxLevel];

        float containerWidth = _container.rect.width - _leftPadding - _rightPadding;
        float containerHeight = _container.rect.height - _topPadding - _bottomPadding;

        float elementWidth = (containerWidth - (_spacing * (maxLevel - 1))) / maxLevel;
        float elementHeight = containerHeight;

        for (int i = 0; i < maxLevel; i++)
        {
            GameObject levelObject = new GameObject("Level_" + i, typeof(Image));
            levelObject.transform.SetParent(_container, false);

            RectTransform rectTransform = levelObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(elementWidth, elementHeight);
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.anchorMax = new Vector2(0, 0.5f);
            rectTransform.pivot = new Vector2(0, 0.5f);

            rectTransform.anchoredPosition = new Vector2(_leftPadding + i * (elementWidth + _spacing), 0f);

            Image image = levelObject.GetComponent<Image>();
            image.sprite = _emptySprite;
            image.color = _emptyColor;

            _levelImages[i] = image;
        }

        await UniTask.CompletedTask;
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
