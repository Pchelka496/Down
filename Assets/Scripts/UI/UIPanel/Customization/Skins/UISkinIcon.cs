using UnityEngine;
using UnityEngine.UI;

namespace UI.UIPanel.Customization.Skins
{
    public class UISkinIcon : MonoBehaviour
    {
        [SerializeField] RectTransform _rectTransform;
        [SerializeField] Button _button;

        public RectTransform RectTransform => _rectTransform;

        public Button Button => _button;

        private void Reset()
        {
            _rectTransform = gameObject.GetComponent<RectTransform>();
            _button = gameObject.GetComponent<Button>();
        }
    }
}