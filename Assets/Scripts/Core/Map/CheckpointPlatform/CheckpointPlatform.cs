using Core;
using Creatures.Player;
using UnityEngine;
using Zenject;

public class CheckpointPlatform : MonoBehaviour
{
    const float BORDER_POSITION_OFFSET = 0.5f;
    [SerializeField] Canvas _canvas;
    [SerializeField] DisplayController _displayController;
    [SerializeField] BoxCollider2D _collider;

    PlayerController _player;
    GlobalEventsManager _globalEventsManager;
    LobbyUIPanelFacade _lobbyUIPanelFacade;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(GlobalEventsManager globalEventsManager, PlayerController player, CameraFacade cameraFacade, LobbyUIPanelFacade upgradePanelController)
    {
        _globalEventsManager = globalEventsManager;
        _player = player;
        _canvas.worldCamera = cameraFacade.Camera;
        _lobbyUIPanelFacade = upgradePanelController;

        (var width, var height) = GetCanvasSizeForCamera(cameraFacade.Camera, cameraFacade.LobbyOrthographicSize);

        ResizeToCamera(width, height);
    }

    private (float width, float height) GetCanvasSizeForCamera(Camera camera, float orthographicSize)
    {
        if (!camera.orthographic)
        {
            Debug.LogError("Camera is not orthographic");
            return (0f, 0f);
        }

        var height = orthographicSize * 2f;

        var width = height * camera.aspect;

        return (width, height);
    }

    private void ResizeToCamera(float width, float height)
    {
        //Vector3 bottomLeft = camera.ViewportToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
        //Vector3 topRight = camera.ViewportToWorldPoint(new Vector3(1, 1, camera.nearClipPlane));

        //float width = topRight.x - bottomLeft.x;
        //float height = topRight.y - bottomLeft.y;

        _collider.size = new Vector2(width, height);

        RectTransform canvasRect = _canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(width, height);
        canvasRect.position = Vector3.zero;

        CreateBorder(width, height);
    }

    private void CreateBorder(float width, float height)
    {
        var borderThickness = 0.2f;
        var halfWidth = width * 0.5f;
        var halfHeight = height * 0.5f;

        var rightBorder = gameObject.AddComponent<BoxCollider2D>();
        var leftBorder = gameObject.AddComponent<BoxCollider2D>();
        var topBorder = gameObject.AddComponent<BoxCollider2D>();

        rightBorder.offset = new(halfWidth + BORDER_POSITION_OFFSET, 0f);
        leftBorder.offset = new(-(halfWidth + BORDER_POSITION_OFFSET), 0f);
        topBorder.offset = new(0f, halfHeight + BORDER_POSITION_OFFSET);

        rightBorder.size = new(borderThickness, height);
        leftBorder.size = new(borderThickness, height);
        topBorder.size = new(width, borderThickness);
    }

    public void Initialize(Initializer initializer)
    {
        transform.position = new(0f, initializer.PlatformHeight);
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == _player.gameObject)
        {
            _globalEventsManager.PlayerLeftThePlatform();
        }
    }

    public void OpenUpgradePanel()
    {
        _lobbyUIPanelFacade.OpenUpgradePanel();
    }

    public void OpenCustomizationPanel()
    {
        _lobbyUIPanelFacade.OpenCustomizationPanel();
    }

    public void OpenWarpEngineControllerPanel()
    {
        _lobbyUIPanelFacade.OpenWarpEngineController();
    }

    public readonly struct Initializer
    {
        public readonly float PlatformHeight;

        public Initializer(float platformHeight)
        {
            PlatformHeight = platformHeight;
        }
    }

}

