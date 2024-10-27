using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class CheckpointPlatform : MonoBehaviour
{
    const float BORDER_POSITION_OFFSET = 0.5f;
    const float PLAYER_ROTATION_SPEED_AT_TOUCH_BORDER = 10f;
    [SerializeField] Canvas _canvas;
    [SerializeField] DisplayController _displayController;
    [SerializeField] BoxCollider2D _collider;
    [SerializeField] Vector2 _offsetDistance = new(5f, 10f);

    CheckpointPlatformController _platformController;
    CharacterController _player;
    MapController _mapController;
    LevelManager _levelManager;
    CamerasController _camerasController;
    UpgradePanelController _upgradePanelController;

    [Inject]
    private void Construct(MapController mapController, LevelManager levelManager, CharacterController player, Camera camera, CamerasController camerasController, UpgradePanelController upgradePanelController)
    {
        _mapController = mapController;
        _levelManager = levelManager;
        _player = player;
        _canvas.worldCamera = camera;
        _camerasController = camerasController;
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

    public void Initialize(Initializer initializer, CheckpointPlatformController platformController)
    {
        transform.position = new(0f, initializer.PlatformHeight);
        _platformController = platformController;
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == _player.gameObject)
        {
            _levelManager.RoundStart().Forget();
        }
    }

    public void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject == _player.gameObject)
        {
            _player.transform.Rotate(0, 0, PLAYER_ROTATION_SPEED_AT_TOUCH_BORDER * Time.deltaTime);
        }
    }

    public async void OpenUpgradePanel()
    {
        await UniTask.WaitForSeconds(0.5f);

        _upgradePanelController.UpgradePanel.OpenPanel();
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

