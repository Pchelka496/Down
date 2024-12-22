using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialPanel : MonoBehaviour, IUIPanel
{
    [Header("Slides Settings")]
    [SerializeField] GameObject[] slides;
    [SerializeField] Button nextButton;
    [SerializeField] Button previousButton;
    [SerializeField] CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    [SerializeField] float buttonAnimationDuration = 0.3f;
    [SerializeField] float _transitionDuration = 0.5f;

    int _currentSlideIndex = 0;

    private void Awake()
    {
        if (slides == null || slides.Length == 0)
        {
            Debug.LogError("Slides are not assigned or empty.");
            return;
        }

        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup is not assigned.");
            return;
        }

        if (nextButton != null)
            nextButton.onClick.AddListener(ShowNextSlide);

        if (previousButton != null)
            previousButton.onClick.AddListener(ShowPreviousSlide);

        InitializeSlides();
        UpdateSlidesVisibility();
    }

    void IUIPanel.Open()
    {
        gameObject.SetActive(true);

        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            transform.SetParent(canvas.transform);
            transform.SetAsLastSibling();
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void ShowNextSlide()
    {
        if (_currentSlideIndex < slides.Length - 1)
        {
            TransitionSlide(_currentSlideIndex, _currentSlideIndex + 1);
            _currentSlideIndex++;
            UpdateSlidesVisibility();
        }
    }

    private void ShowPreviousSlide()
    {
        if (_currentSlideIndex > 0)
        {
            TransitionSlide(_currentSlideIndex, _currentSlideIndex - 1);
            _currentSlideIndex--;
            UpdateSlidesVisibility();
        }
    }

    private void TransitionSlide(int fromIndex, int toIndex)
    {
        canvasGroup.DOFade(0f, _transitionDuration).OnComplete(() =>
        {
            slides[fromIndex].SetActive(false);
            slides[toIndex].SetActive(true);
            canvasGroup.DOFade(1f, _transitionDuration);
        });
    }

    private void UpdateSlidesVisibility()
    {
        AnimateButtonState(previousButton, _currentSlideIndex > 0);

        AnimateButtonState(nextButton, _currentSlideIndex < slides.Length - 1);
    }

    private void AnimateButtonState(Button button, bool enable)
    {
        if (button == null) return;

        if (!button.TryGetComponent<RectTransform>(out var buttonTransform)) return;

        if (enable)
        {
            button.gameObject.SetActive(true);
            buttonTransform.DOScale(1.1f, buttonAnimationDuration / 2)
                .OnComplete(() => buttonTransform.DOScale(1f, buttonAnimationDuration / 2));
        }
        else
        {
            buttonTransform.DOScale(1.1f, buttonAnimationDuration / 2)
                .OnComplete(() =>
                {
                    buttonTransform.DOScale(0f, buttonAnimationDuration / 2).OnComplete(() =>
                    {
                        button.gameObject.SetActive(false);
                    });
                });
        }
    }

    private void InitializeSlides()
    {
        for (int i = 0; i < slides.Length; i++)
        {
            slides[i].SetActive(i == _currentSlideIndex);
        }

        canvasGroup.alpha = 1f;
    }
}
