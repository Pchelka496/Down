using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;
using System.Runtime.CompilerServices;
public class CheckpointPlatform : MonoBehaviour
{
    const float PLATFORM_PIECE_Y_POSITION_OFFSET = 1f;

    [SerializeField] EdgeCollider2D _edgeCollider;
    [SerializeField] float _colliderHeight;
    [SerializeField] RectTransform _worldCanvas;
    [SerializeField] float _canvasSizeDeltaY;
    [SerializeField] TextMeshProUGUI _timer;
    [SerializeField] string _startText = "Go!";

    [SerializeField] float _waitTimeForStartNewLevel;
    [SerializeField] string _platformPieceAddress;
    [SerializeField] GameObject[] _objectsOnDisabled;
    [SerializeField] Queue<PlatformPiece> _platformPieces = new();

    float[] _doorLocalXPosition;
    MapController _mapController;
    LevelManager _levelManager;
    float _levelWidth;
    bool _savingOption = false;
    CancellationTokenSource _nextLevelTransitions;
    CancellationTokenSource _timerMovement;

    [Inject]
    private void Construct(MapController mapController, LevelManager levelManager)
    {
        _mapController = mapController;
        _levelManager = levelManager;
    }

    private void Start()
    {
        _nextLevelTransitions = new CancellationTokenSource();
        _timerMovement = new CancellationTokenSource();
    }

    public void Initialize(Initializer initializer)
    {
        transform.position = new(0f, initializer.PlatformHeight);
        _timer.text = "";
        _edgeCollider.enabled = true;
        ResizeAsync(initializer.PlatformWidth).Forget();
        _doorLocalXPosition = initializer.DoorLocalXPosition;

        PlatformActivation(initializer.PlatformHeight, initializer.PlatformWidth);
        _savingOption = initializer.SavingOption;
    }

    private void PlatformActivation(float platformHeight, float platformWidth)
    {
        if (_objectsOnDisabled != null)
        {
            foreach (var @object in _objectsOnDisabled)
            {
                @object.SetActive(true);
            }
        }
    }

    private async UniTask ResizeAsync(float fullPlatformSize)
    {
        var platformPiece = await LoadPlatformPiece(_platformPieceAddress);

        if (platformPiece == null) return;

        ClearPlatformPieces();

        ColliderSetting(fullPlatformSize);
        CanvasSetting(fullPlatformSize);
        CreatePlatformPieces(platformPiece, fullPlatformSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ColliderSetting(float fullPlatformSize)
    {
        _edgeCollider.offset = new(0f, _colliderHeight);
        _edgeCollider.points = new Vector2[2]
         {
            new Vector2(fullPlatformSize * 0.5f, 0f),
            new Vector2(-fullPlatformSize * 0.5f, 0f)
         };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CanvasSetting(float fullPlatformSize)
    {
        _worldCanvas.sizeDelta = new(fullPlatformSize, _canvasSizeDeltaY);
    }

    private void CreatePlatformPieces(PlatformPiece platformPiece, float fullPlatformSize)
    {
        var pieceSize = platformPiece.Size;

        int pieceCount = Mathf.CeilToInt(fullPlatformSize / pieceSize.x);

        Vector2 basePosition = new(transform.position.x, transform.position.y - pieceSize.y / 2 - PLATFORM_PIECE_Y_POSITION_OFFSET);

        var centerPiece = Instantiate(platformPiece, basePosition, Quaternion.identity, transform);
        _platformPieces.Enqueue(centerPiece);

        for (int i = 1; i <= pieceCount / 2; i++)
        {
            Vector2 leftPosition = basePosition - new Vector2(i * pieceSize.x, 0);
            var leftPiece = Instantiate(platformPiece, leftPosition, Quaternion.identity, transform);
            _platformPieces.Enqueue(leftPiece);

            Vector2 rightPosition = basePosition + new Vector2(i * pieceSize.x, 0);
            var rightPiece = Instantiate(platformPiece, rightPosition, Quaternion.identity, transform);
            _platformPieces.Enqueue(rightPiece);
        }
    }

    private void ClearPlatformPieces()
    {
        foreach (var platformPieces in _platformPieces)
        {
            if (platformPieces != null)
            {
                Destroy(platformPieces.gameObject);
            }
        }
    }

    private async UniTask<PlatformPiece> LoadPlatformPiece(string address)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(address);

        await handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var platformPieceGO = handle.Result;

            if (platformPieceGO.TryGetComponent<PlatformPiece>(out var platformPiece))
            {
                return platformPiece;
            }
            else
            {
                Debug.LogError("Failed to retrieve PlatformPiece component.");
                return null;
            }
        }
        else
        {
            Debug.LogError("Error loading platform via Addressables.");
            return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<CharacterController>(out var player))
        {
            if (_nextLevelTransitions != null && !_nextLevelTransitions.IsCancellationRequested)
            {
                CancelNextLevelTransitions();
            }

            PlayerSaving();

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<CharacterController>(out var player))
        {
            _nextLevelTransitions?.Cancel();
            _nextLevelTransitions?.Dispose();
            _timerMovement?.Cancel();
            _timerMovement?.Dispose();

            _nextLevelTransitions = new CancellationTokenSource();
            _timerMovement = new CancellationTokenSource();

            NextLevelStart(_nextLevelTransitions.Token).Forget();
            TimerFollowing(player.transform, 2f, 10f, 0.5f, _timerMovement.Token).Forget();
        }
    }

    private async UniTask TimerFollowing(Transform followTransform, float baseSpeed, float maxSpeed, float followDistance, CancellationToken cancellationToken)
    {
        var timerTransform = _timer.rectTransform;

        while (!cancellationToken.IsCancellationRequested)
        {
            var deltaTime = Time.deltaTime;
            var targetX = followTransform.position.x;
            var currentX = timerTransform.position.x;

            var distance = Mathf.Abs(targetX - currentX);

            if (distance > followDistance)
            {
                var speed = Mathf.Min(baseSpeed * (distance / followDistance), maxSpeed);
                var newX = Mathf.Lerp(currentX, targetX, speed * deltaTime);

                timerTransform.position = new Vector3(newX, timerTransform.position.y, timerTransform.position.z);
            }

            await UniTask.WaitForSeconds(deltaTime, cancellationToken: cancellationToken);
        }
    }

    private void PlayerSaving()
    {
        if (!_savingOption) return;
        _savingOption = false;
        var heightOffset = 10f;

        _levelManager.PlayerSavedHeight = transform.position.y + heightOffset;
        _levelManager.SwitchToNextLevel();
    }

    private void CancelNextLevelTransitions()
    {
        _nextLevelTransitions.Cancel();
        _timer.text = "";
    }

    private async UniTask NextLevelStart(CancellationToken cancellationToken)
    {
        float remainingTime = _waitTimeForStartNewLevel;
        _timer.text = "";

        while (remainingTime > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _timer.text = Mathf.CeilToInt(remainingTime).ToString();
            await UniTask.Delay(1000, cancellationToken: cancellationToken);
            remainingTime--;
        }

        cancellationToken.ThrowIfCancellationRequested();

        _timer.text = _startText;
        _edgeCollider.enabled = false;
        PlatformDeactivation();

        _levelManager.StartNextLevel();

        await UniTask.CompletedTask;
    }

    private void PlatformDeactivation()
    {
        DisableClosestPlatformPieces();
        Dispose();

        if (_objectsOnDisabled != null)
        {
            foreach (var @object in _objectsOnDisabled)
            {
                @object.SetActive(false);
            }
        }
    }

    private void DisableClosestPlatformPieces()
    {
        foreach (var doorX in _doorLocalXPosition)
        {
            PlatformPiece closestPiece = null;
            float closestDistance = float.MaxValue;

            foreach (var piece in _platformPieces)
            {
                if (piece == null) continue;

                float pieceX = piece.transform.localPosition.x;
                float distance = Mathf.Abs(pieceX - doorX);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPiece = piece;
                }
            }

            if (closestPiece != null)
            {
                closestPiece.gameObject.SetActive(false);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Dispose()
    {
        if (_nextLevelTransitions != null && !_nextLevelTransitions.IsCancellationRequested)
        {
            _nextLevelTransitions.Cancel();
        }

        if (_nextLevelTransitions != null)
        {
            _nextLevelTransitions.Dispose();
            _nextLevelTransitions = null;
        }

        if (_timerMovement != null && !_timerMovement.IsCancellationRequested)
        {
            _timerMovement.Cancel();
        }

        if (_timerMovement != null)
        {
            _timerMovement.Dispose();
            _timerMovement = null;
        }
    }


    private void OnDestroy()
    {
        Dispose();
    }

#if UNITY_EDITOR
    [Header("UNITY_EDITOR")]
    [SerializeField] float _width;

    [ContextMenu("Resize")]
    public void Resize()
    {
        _ = ResizeAsync(_width);
    }

    [ContextMenu("ClearPlatformPieces")]
    public void Clear()
    {
        while (_platformPieces.Count > 0)
        {
            var platformPiece = _platformPieces.Dequeue();

            if (platformPiece != null && platformPiece.gameObject != null)
            {
                DestroyImmediate(platformPiece.gameObject);
            }
        }
    }
#endif

    public readonly struct Initializer
    {
        public readonly float PlatformHeight;
        public readonly float PlatformWidth;
        public readonly float[] DoorLocalXPosition;
        public readonly bool SavingOption;

        public Initializer(float platformHeight, float platformWidth, float[] doorLocalXPosition, bool savingOption)
        {
            PlatformHeight = platformHeight;
            PlatformWidth = platformWidth;
            DoorLocalXPosition = doorLocalXPosition;
            SavingOption = savingOption;
        }

    }

}

