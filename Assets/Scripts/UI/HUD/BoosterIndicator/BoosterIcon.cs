using UnityEngine;

public class BoosterIcon : MonoBehaviour
{
    [SerializeField] RectTransform _rectTransform;
    [SerializeField] Animator _animator;

    public RectTransform RectTransform => _rectTransform;

    public void Activate()
    {
        _animator.Play("Enable");
    }

    public void Deactivate()
    {
        _animator.Play("Disable");
    }

}
