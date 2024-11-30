using UnityEngine;
using UnityEngine.UI;

public class UISkinIcon : MonoBehaviour
{
    [SerializeField] RectTransform _rectTransform;
    [SerializeField] Button _button;

    public RectTransform RectTransform { get => _rectTransform; }
    public Button Button { get => _button; }

    private void Reset()
    {
        _rectTransform = gameObject.GetComponent<RectTransform>();
        _button = gameObject.GetComponent<Button>();
    }

}
