using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] Image _image;
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] float _fadeDuration = 1f;
    [SerializeField] AnimationCurve _fadeCurve;

    private void Awake()
    {
        _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 1f);
        _image.gameObject.SetActive(true);
    }

    public async UniTask FadeToBlack()
    {
        _canvasGroup.blocksRaycasts = false;
        _image.gameObject.SetActive(true);

        _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 1f);

        var tween = _image.DOFade(1f, _fadeDuration).SetEase(_fadeCurve).OnComplete(() =>
         {

         });

        await tween.AsyncWaitForCompletion();
    }

    public async UniTask FadeToClear()
    {
        var tween = _image.DOFade(0f, _fadeDuration).SetEase(_fadeCurve).OnComplete(() =>
          {
              _canvasGroup.blocksRaycasts = true;
              _image.gameObject.SetActive(false);
          });

        await tween.AsyncWaitForCompletion();
    }

}
