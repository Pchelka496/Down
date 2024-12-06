using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.Serialization;
using Zenject;

public class ScreenTouchController : OnScreenButton, IPointerDownHandler, IPointerUpHandler, IDragHandler, IHaveControllerGradient
{
    [FormerlySerializedAs("lineRenderer")] [SerializeField] LineRenderer _lineRenderer;
    Camera _mainCamera;
    Vector2 _touchStartPosition;
    Vector2 _touchEndPosition;
    Vector2 _currentTouchPosition;

    public Vector2 TouchStartPosition => _touchStartPosition;
    public Vector2 TouchEndPosition => _touchEndPosition;
    public Vector2 TouchCurrentPosition => _currentTouchPosition;

    Gradient IHaveControllerGradient.ControllerGradient { get => _lineRenderer.colorGradient; set => _lineRenderer.colorGradient = value; }

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:������� �������������� �������� �����", Justification = "<��������>")]
    private void Construct(Camera mainCamera)
    {
        _mainCamera = mainCamera;
    }

    private void Start()
    {
        ClearLineRenderer();
    }

    public new void OnPointerDown(PointerEventData eventData)
    {
        _touchStartPosition = eventData.position;
        _currentTouchPosition = eventData.position;

        UpdateLineRendererStartPosition();
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
        UpdateLineRendererEndPosition();
    }

    private void UpdateLineRendererStartPosition()
    {
        var worldStart = _mainCamera.ScreenToWorldPoint(new(_touchStartPosition.x, _touchStartPosition.y, 10f));
        var localStart = _lineRenderer.transform.InverseTransformPoint(worldStart);

        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, localStart);
        _lineRenderer.SetPosition(1, localStart);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateLineRendererEndPosition()
    {
        var worldCurrent = _mainCamera.ScreenToWorldPoint(new(_currentTouchPosition.x, _currentTouchPosition.y, 10f));
        var localCurrent = _lineRenderer.transform.InverseTransformPoint(worldCurrent);

        var worldStart = _mainCamera.ScreenToWorldPoint(new(_touchStartPosition.x, _touchStartPosition.y, 10f));
        var localStart = _lineRenderer.transform.InverseTransformPoint(worldStart);

        _lineRenderer.SetPosition(0, localStart);
        _lineRenderer.SetPosition(1, localCurrent);
    }

    private void ClearLineRenderer()
    {
        _lineRenderer.positionCount = 0;
    }

}



