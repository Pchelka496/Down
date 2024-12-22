using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using Types.record;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Zenject;

namespace UI.UIPanel.Customization.Skins
{
    public class SkinsCustomizer : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] ScrollRect _scrollRect;

        [SerializeField] SelectedItemBackground _selectedItemBackground;
        [SerializeField] RectTransform _content;
        [SerializeField] UnlockSkinButtonController _buttonController;

        [Header("Layout Settings")]
        [SerializeField] float _itemSpacing = 10f;

        [SerializeField] Vector2 _itemPadding = new(20f, 20f);
        [SerializeField] float _itemSize = 200f;
        [SerializeField] float _selectedItemBackgroundSize = 220f;

        PlayerSkinData _selectedSkinData;
        IHaveSkin _haveSkin;

        readonly LinkedList<AsyncOperationHandle<GameObject>> _allUISkinIconLoadHandle = new();
        readonly LinkedList<UISkinIcon> _allUISkinIcons = new();

        private void Awake()
        {
            _selectedItemBackground.Initialize(_selectedItemBackgroundSize);

            _buttonController.Initialize(OnUnlockButtonClick, OnApplyButtonClick);

            SetSelectedSkinData();
        }

        public void Initialize(IHaveSkin skin, IHaveAllPlayerSkins allPlayerSkins)
        {
            ClearContent();

            _haveSkin = skin;
            var skins = allPlayerSkins.AllPlayerSkins;

            var totalWidth = (skins.Length * _itemSize) + ((skins.Length - 1) * _itemSpacing) + _itemPadding.x * 2;
            _content.sizeDelta = new Vector2(totalWidth, _content.sizeDelta.y);
            _content.position = new Vector2(totalWidth, _content.position.y);

            for (int i = 0; i < skins.Length; i++)
            {
                CreateSkinUIItem(skins[i], i).Forget();
            }
        }

        private async UniTask CreateSkinUIItem(PlayerSkinData skinData, int index)
        {
            var loadData = await AddressableLouderHelper.LoadAssetAsync<GameObject>(skinData.UISkinIcon);

            _allUISkinIconLoadHandle.AddFirst(loadData.Handle);

            var icon = Instantiate(loadData.LoadAsset).GetComponent<UISkinIcon>();

            _allUISkinIcons.AddLast(icon);

            IconRectTransformSetting(icon.RectTransform, index);
            IconButtonSetting(icon.Button, skinData);
        }

        private void IconRectTransformSetting(RectTransform rectTransform, int index)
        {
            rectTransform.SetParent(_content, false);

            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.anchorMax = new Vector2(0, 0.5f);
            rectTransform.pivot = new Vector2(0, 0.5f);

            rectTransform.sizeDelta = new Vector2(_itemSize, _itemSize);

            var initialPosition = new Vector2(
                x: index * (_itemSize + _itemSpacing) + _itemPadding.x,
                y: 0);

            rectTransform.anchoredPosition = initialPosition;

            var pivotOffset = rectTransform.sizeDelta * (rectTransform.pivot - new Vector2(0, 0.5f));
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition += pivotOffset;
        }

        private void IconButtonSetting(Button button, PlayerSkinData skinData)
        {
            button.onClick.AddListener(() => { SetSelectedSkinData(skinData, button.transform.position); });
        }

        private void SetSelectedSkinData(PlayerSkinData skinData = null, Vector2? iconGlobalPosition = null)
        {
            ConfigureSelectedSkinBackground(skinData, iconGlobalPosition);

            _buttonController.UpdateSkinData(skinData);

            _selectedSkinData = skinData;
        }

        private void ConfigureSelectedSkinBackground(PlayerSkinData skinData, Vector2? iconGlobalPosition)
        {
            if (skinData == null)
            {
                _selectedItemBackground.OnSkinUnselected();
                return;
            }

            if (iconGlobalPosition != null)
            {
                _selectedItemBackground.OnSkinSelected(iconGlobalPosition.Value);
            }
        }

        private void OnApplyButtonClick()
        {
            _haveSkin.Skin = _selectedSkinData.SkinObjectPrefab;
        }

        private void OnUnlockButtonClick()
        {
            var unlockStatus = _selectedSkinData.UnlockMethods()?.Invoke();

            if (unlockStatus.HasValue && unlockStatus.Value)
            {
                SetSelectedSkinData(_selectedSkinData);
            }
        }

        private void ClearContent()
        {
            foreach (var icon in _allUISkinIcons)
            {
                Destroy(icon.gameObject);
            }

            foreach (var handle in _allUISkinIconLoadHandle)
            {
                if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Addressables.Release(handle);
                }
            }
        }

        private void OnDestroy()
        {
            ClearContent();
        }
    }
}
