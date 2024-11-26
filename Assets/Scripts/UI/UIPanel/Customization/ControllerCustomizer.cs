using UnityEngine;
using UnityEngine.UI;

public class ControllerCustomizer : MonoBehaviour
{
    [SerializeField] Image _viewControllerGradient;
    [SerializeField] ScreenPipette _screenPipette;

    [SerializeField] Button _startColorButton;
    [SerializeField] Button _endColorButton;

    IHaveControllerGradient _controllerGradient;
    Gradient _gradient = new();
    bool _isPickingStartColor = true;

    public Gradient Gradient
    {
        set
        {
            _gradient = value;
            UpdateGradientView(_gradient);

            if (_controllerGradient != null)
            {
                _controllerGradient.ControllerGradient = value;
            }
        }
    }

    public void Initialize(IHaveControllerGradient controllerGradient)
    {
        _controllerGradient = controllerGradient;
        Gradient = controllerGradient.ControllerGradient;
    }

    private void OnEnable()
    {
        _startColorButton.onClick.AddListener(SetChangeStartColorMode);
        _endColorButton.onClick.AddListener(SetChangeEndColorMode);
    }

    private void OnDisable()
    {
        _startColorButton.onClick.RemoveListener(SetChangeStartColorMode);
        _endColorButton.onClick.RemoveListener(SetChangeEndColorMode);

        _screenPipette.UnsubscribeFromOnColorPicked(ColorPicked);
        _screenPipette.UnsubscribeFromOnColorPickedEnd(ColorPickedEnd);
    }

    private void SetChangeStartColorMode()
    {
        _isPickingStartColor = true;
        EnableColorPicker();
    }

    private void SetChangeEndColorMode()
    {
        _isPickingStartColor = false;
        EnableColorPicker();
    }

    private void EnableColorPicker()
    {
        _screenPipette.SubscribeToOnColorPicked(ColorPicked);
        _screenPipette.SubscribeToOnColorPickedEnd(ColorPickedEnd);
    }

    private void ColorPicked(Color color)
    {
        SetColorOnGradient(color);
    }

    private void ColorPickedEnd(Color color)
    {
        _screenPipette.UnsubscribeFromOnColorPicked(ColorPicked);
        _screenPipette.UnsubscribeFromOnColorPickedEnd(ColorPickedEnd);

        SetColorOnGradient(color);
    }

    private void SetColorOnGradient(Color color)
    {
        var colorKeys = _gradient.colorKeys;

        if (_isPickingStartColor)
        {
            colorKeys[0].color = color;
        }
        else
        {
            colorKeys[^1].color = color;
        }

        _gradient.SetKeys(colorKeys, _gradient.alphaKeys);

        Gradient = _gradient;
    }

    private void UpdateGradientView(Gradient gradient)
    {
        var width = (int)_viewControllerGradient.rectTransform.rect.width;
        var height = (int)_viewControllerGradient.rectTransform.rect.height;

        GradientExample.ApplyTextureToImage(_viewControllerGradient,
                                            GradientExample.GenerateGradientTexture(gradient,
                                                                                    width,
                                                                                    height,
                                                                                    GradientExample.GradientDirection.Horizontal));
    }

}


