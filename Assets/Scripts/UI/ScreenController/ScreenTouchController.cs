using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using Zenject;

public class ScreenTouchController : OnScreenButton, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] LineRenderer lineRenderer;
    Camera _mainCamera;
    Vector2 _touchStartPosition;
    Vector2 _touchEndPosition;
    Vector2 _currentTouchPosition;

    public Vector2 TouchStartPosition => _touchStartPosition;
    public Vector2 TouchEndPosition => _touchEndPosition;
    public Vector2 TouchCurrentPosition => _currentTouchPosition;

    [Inject]
    private void Construct(Camera mainCamera)
    {
        _mainCamera = mainCamera;
    }

    public new void OnPointerDown(PointerEventData eventData)
    {
        _touchStartPosition = eventData.position;
        UpdateLineRendererStartPosition();
        base.OnPointerDown(eventData);
    }

    public new void OnPointerUp(PointerEventData eventData)
    {
        _touchEndPosition = eventData.position;
        ClearLineRenderer();
        base.OnPointerUp(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        _currentTouchPosition = eventData.position;
        UpdateLineRendererEndPosition();
    }

    private void UpdateLineRendererStartPosition()
    {
        Vector3 worldStart = _mainCamera.ScreenToWorldPoint(new Vector3(_touchStartPosition.x, _touchStartPosition.y, 10f));
        Vector3 localStart = lineRenderer.transform.InverseTransformPoint(worldStart);

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, localStart);
        lineRenderer.SetPosition(1, localStart);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateLineRendererEndPosition()
    {
        Vector3 worldCurrent = _mainCamera.ScreenToWorldPoint(new Vector3(_currentTouchPosition.x, _currentTouchPosition.y, 10f));
        Vector3 localCurrent = lineRenderer.transform.InverseTransformPoint(worldCurrent);

        lineRenderer.SetPosition(1, localCurrent);
    }

    private void ClearLineRenderer()
    {
        lineRenderer.positionCount = 0;
    }

}



