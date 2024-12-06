using UnityEngine;

namespace UI.UIPanel.Customization.Skins
{
    public class SelectedItemBackground : MonoBehaviour
    {
        [SerializeField] RectTransform _rectTransform;

        public void Initialize(float size)
        {
            _rectTransform.sizeDelta = new Vector2(size, size);
        }

        public void OnSkinSelected(Vector2 skinPosition)
        {
            transform.position = skinPosition;
            gameObject.SetActive(true);
        }

        public void OnSkinUnselected()
        {
            gameObject.SetActive(false);
        }

        private void Reset()
        {
            _rectTransform = GetComponent<RectTransform>();
        }
    }
}