using UnityEngine;
using Zenject;

public class CheckpointPlatform : MonoBehaviour
{
    const float BORDER_POSITION_OFFSET = 0.5f;
    [SerializeField] Canvas _canvas;
    [SerializeField] DisplayController _displayController;
    [SerializeField] BoxCollider2D _collider;

    PlayerController _player;
    LevelManager _levelManager;
    LobbyUIPanelFacade _upgradePanelController;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(LevelManager levelManager, PlayerController player, Camera camera, LobbyUIPanelFacade upgradePanelController)
    {
        _levelManager = levelManager;
        _player = player;
        _canvas.worldCamera = camera;
        _upgradePanelController = upgradePanelController;

        ResizeToCamera(camera);
    }

    private void ResizeToCamera(Camera camera)
    {
        Vector3 bottomLeft = camera.ViewportToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
        Vector3 topRight = camera.ViewportToWorldPoint(new Vector3(1, 1, camera.nearClipPlane));

        float width = topRight.x - bottomLeft.x;
        float height = topRight.y - bottomLeft.y;

        _collider.size = new Vector2(width, height);

        RectTransform canvasRect = _canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(width, height);
        canvasRect.position = Vector3.zero;

        CreateBorder(width, height);
    }

    private void CreateBorder(float width, float height)
    {
        var borderThickness = 0.2f;
        var rightBorder = gameObject.AddComponent<BoxCollider2D>();
        var leftBorder = gameObject.AddComponent<BoxCollider2D>();
        var topBorder = gameObject.AddComponent<BoxCollider2D>();

        rightBorder.offset = new(width / 2 + BORDER_POSITION_OFFSET, 0f);
        leftBorder.offset = new(-(width / 2 + BORDER_POSITION_OFFSET), 0f);
        topBorder.offset = new(0f, height / 2 + BORDER_POSITION_OFFSET);

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
            _levelManager.RoundStart().Forget();
        }
    }

    public void OpenUpgradePanel()
    {
        _upgradePanelController.OpenUpgradePanel();
    }

    public void OpenCustomizationPanel()
    {
        _upgradePanelController.OpenCustomizationPanel();
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

