using Core.Installers;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;

public class ScreenTouchController : OnScreenButton, IPointerDownHandler, IPointerUpHandler, IDragHandler, IHaveControllerGradient
{
    const float LINE_RENDER_POINT_Z_POSITION = 10f;
    [SerializeField] Camera _camera;
    [SerializeField] LineRenderer _lineRenderer;
    Transform _lineRenderTransform;

    Vector2 _touchStartPosition;
    Vector2 _touchEndPosition;
    Vector2 _currentTouchPosition;

    public Vector2 TouchStartPosition => _touchStartPosition;
    public Vector2 TouchEndPosition => _touchEndPosition;
    public Vector2 TouchCurrentPosition => _currentTouchPosition;

    Gradient IHaveControllerGradient.ControllerGradient { get => _lineRenderer.colorGradient; set => _lineRenderer.colorGradient = value; }

    private void Start()
    {
        _lineRenderTransform = _lineRenderer.transform;
        ClearLineRenderer();
    }

    public new void OnPointerDown(PointerEventData eventData)
    {
        _touchStartPosition = eventData.position;
        _currentTouchPosition = eventData.position;

        UpdateLineRendererStartPosition();
        UpdateLineRendererCurrentPosition();

        base.OnPointerDown(eventData);
    }

    public new void OnPointerUp(PointerEventData eventData)
    {
        _touchEndPosition = eventData.position;
        _currentTouchPosition = eventData.position;

        ClearLineRenderer();
        base.OnPointerUp(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        _currentTouchPosition = eventData.position;
        UpdateLineRendererCurrentPosition();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateLineRendererStartPosition()
    {
        _lineRenderer.positionCount = 2;

        var startTouchLocalPosition = _lineRenderTransform.InverseTransformPoint(_camera.ScreenToWorldPoint(_touchStartPosition));

        _lineRenderer.SetPosition(0, new(startTouchLocalPosition.x, startTouchLocalPosition.y, 10f));
        _lineRenderer.SetPosition(1, new(startTouchLocalPosition.x, startTouchLocalPosition.y, 10f));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateLineRendererCurrentPosition()
    {
        var currentTouchLocalPosition = _lineRenderTransform.InverseTransformPoint(_camera.ScreenToWorldPoint(_currentTouchPosition));

        _lineRenderer.SetPosition(1, new(currentTouchLocalPosition.x, currentTouchLocalPosition.y, LINE_RENDER_POINT_Z_POSITION));
    }

    private void ClearLineRenderer()
    {
        _lineRenderer.positionCount = 0;
    }
}
