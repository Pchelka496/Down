using UnityEngine;

public class BoosterIcon : MonoBehaviour
{
    [SerializeField] RectTransform _rectTransform;
    [SerializeField] Animator _animator;

    public RectTransform RectTransform => _rectTransform;

    public void Activate()
    {
        if (!_animator.enabled) return;

        _animator.Play("Enable");
    }

    public void Deactivate()
    {
        if (!_animator.enabled) return;

        _animator.Play("Disable");
    }
}
