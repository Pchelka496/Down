using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Nenn.InspectorEnhancements.Runtime.Attributes;

public class TutorialPanel : MonoBehaviour, IUIPanel
{
    [Header("Slides Settings")]
    [HideLabel][Required][SerializeField] GameObject[] _slides;
    [Required][SerializeField] Button _nextButton;
    [Required][SerializeField] Button _previousButton;
    [Required][SerializeField] CanvasGroup _slidesCanvasGroup;
    [Required][SerializeField] CanvasGroup _globalCanvasGroup;

    [Header("Animation Settings")]
    [SerializeField] float _buttonAnimationDuration = 0.3f;
    [SerializeField] float _enableFadeDuration = 1f;
    [SerializeField] float _slideTransitionDuration = 0.5f;

    int _currentSlideIndex = 0;

    private void Awake()
    {
        if (_slides == null || _slides.Length == 0)
        {
            Debug.LogError("Slides are not assigned or empty.");
            return;
        }

        if (_nextButton != null)
        {
            _nextButton.onClick.AddListener(ShowNextSlide);
        }

        if (_previousButton != null)
        {
            _previousButton.onClick.AddListener(ShowPreviousSlide);
        }

        InitializeSlides();
        UpdateSlidesVisibility();

        _globalCanvasGroup.alpha = 0f;
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

        _globalCanvasGroup.DOFade(1f, _enableFadeDuration);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void ShowNextSlide()
    {
        if (_currentSlideIndex < _slides.Length - 1)
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
        _slidesCanvasGroup.DOFade(0f, _slideTransitionDuration).OnComplete(() =>
        {
            _slides[fromIndex].SetActive(false);
            _slides[toIndex].SetActive(true);
            _slidesCanvasGroup.DOFade(1f, _slideTransitionDuration);
        });
    }

    private void UpdateSlidesVisibility()
    {
        AnimateButtonState(_previousButton, _currentSlideIndex > 0);

        AnimateButtonState(_nextButton, _currentSlideIndex < _slides.Length - 1);
    }

    private void AnimateButtonState(Button button, bool enable)
    {
        if (button == null) return;

        if (!button.TryGetComponent<RectTransform>(out var buttonTransform)) return;

        if (enable)
        {
            button.gameObject.SetActive(true);
            buttonTransform.DOScale(1.1f, _buttonAnimationDuration / 2)
                           .OnComplete(() => buttonTransform.DOScale(1f, _buttonAnimationDuration / 2));
        }
        else
        {
            buttonTransform.DOScale(1.1f, _buttonAnimationDuration / 2)
                .OnComplete(() =>
                {
                    buttonTransform.DOScale(0f, _buttonAnimationDuration / 2)
                    .OnComplete(() =>
                    {
                        button.gameObject.SetActive(false);
                    });
                });
        }
    }

    private void InitializeSlides()
    {
        for (int i = 0; i < _slides.Length; i++)
        {
            _slides[i].SetActive(i == _currentSlideIndex);
        }

        _slidesCanvasGroup.alpha = 1f;
    }
}
