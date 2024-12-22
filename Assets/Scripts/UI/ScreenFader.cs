using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ScreenFader : MonoBehaviour, ITransitionAnimator
    {
        [SerializeField] Image _revealMask;
        [SerializeField] Image _backgroundOverlay;
        [SerializeField] CanvasGroup _canvasGroup;
        [SerializeField] float _fadeDuration = 1f;
        [SerializeField] AnimationCurve _fadeCurve;
        [SerializeField] Vector2 _startSize = new(0, 0);
        readonly Vector2 _endSize = GetCircleBoundingSquareSize();

        EnumState _currentState;
        RectTransform _imageRectTransform;
        event System.Action DisposeEvents;

        [Zenject.Inject]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
        private void Construct(GlobalEventsManager globalEventsManager)
        {
            globalEventsManager.SubscribeToRoundStarted(RoundStart);
            globalEventsManager.SubscribeToRoundEnded(RoundEnd);
            globalEventsManager.SubscribeToFastTravelStarted(FastTravelStart);

            DisposeEvents += () => globalEventsManager?.UnsubscribeFromFastTravelStarted(FastTravelStart);
            DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
            DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);
        }

        private void Awake()
        {
            _backgroundOverlay.rectTransform.sizeDelta = _endSize;

            _imageRectTransform = _revealMask.GetComponent<RectTransform>();
            _imageRectTransform.sizeDelta = _startSize;
            _revealMask.enabled = true;
            _revealMask.gameObject.SetActive(true);
        }

        private void Start()
        {
            FadeToClear().Forget();
        }

        private void FastTravelStart() => _currentState = EnumState.FastTravel;
        private void RoundStart() => _currentState = EnumState.Gameplay;
        private void RoundEnd() => _currentState = EnumState.Lobby;

        async UniTask ITransitionAnimator.PlayStartTransitionAsync()
        {
            switch (_currentState)
            {
                case EnumState.Lobby:
                    {
                        await FadeToBlack();
                        break;
                    }
                case EnumState.Gameplay:
                    {
                        break;
                    }
                case EnumState.FastTravel:
                    {
                        break;
                    }
                default:
                    {
                        Debug.LogError($"Unknown player state: {_currentState}");
                        break;
                    }
            }
        }

        async UniTask ITransitionAnimator.PlayEndTransitionAsync()
        {
            switch (_currentState)
            {
                case EnumState.Lobby:
                    {
                        await FadeToClear();
                        break;
                    }
                case EnumState.Gameplay:
                    {
                        break;
                    }
                case EnumState.FastTravel:
                    {
                        break;
                    }
                default:
                    {
                        Debug.LogError($"Unknown player state: {_currentState}");
                        break;
                    }
            }
        }

        private async UniTask FadeToClear()
        {
            _canvasGroup.blocksRaycasts = true;
            _revealMask.gameObject.SetActive(true);
            _backgroundOverlay.gameObject.SetActive(true);

            _imageRectTransform.sizeDelta = _startSize;

            var tween = _imageRectTransform.DOSizeDelta(_endSize, _fadeDuration)
                .SetEase(_fadeCurve)
                .OnComplete(() =>
                {
                    _revealMask.gameObject.SetActive(false);
                    _backgroundOverlay.gameObject.SetActive(false);
                });

            await tween.AsyncWaitForCompletion();
        }

        public async UniTask FadeToBlack()
        {
            _canvasGroup.blocksRaycasts = true;
            _revealMask.gameObject.SetActive(true);
            _backgroundOverlay.gameObject.SetActive(true);

            var tween = _imageRectTransform.DOSizeDelta(_startSize, _fadeDuration)
                .SetEase(_fadeCurve);

            await tween.AsyncWaitForCompletion();
        }

        private static Vector2 GetCircleBoundingSquareSize()
        {
            float diagonal = Mathf.Sqrt(Mathf.Pow(Screen.width, 2) + Mathf.Pow(Screen.height, 2));
            return new Vector2(diagonal, diagonal);
        }

        private void OnDestroy()
        {
            DisposeEvents?.Invoke();
        }

        private enum EnumState
        {
            Lobby,
            Gameplay,
            FastTravel
        }
    }
}