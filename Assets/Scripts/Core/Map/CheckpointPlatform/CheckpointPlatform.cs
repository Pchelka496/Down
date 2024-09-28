using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using System.Threading;
using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CheckpointPlatform : MonoBehaviour
{
    const float PLATFORM_PIECE_Y_POSITION_OFFSET = 1f;

    [SerializeField] float _waitTimeForStartNewLevel;
    [SerializeField] string _platformPieceAddress;
    float _levelWidth;
    MapController _mapController;
    [SerializeField] Queue<PlatformPiece> _platformPieces = new();
    bool _saveStatus;
    CancellationTokenSource _cancellationTokenSource;

    [Inject]
    private void Construct(MapController mapController)
    {
        _mapController = mapController;

    }

    private void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void Initialize(float platformHeight, float platformWidth)
    {
        transform.position = new(0f, platformHeight);
        _ = ResizeAsync(platformWidth);
    }

    private async UniTask ResizeAsync(float fullPlatformSize)
    {
        var platformPiece = await LoadPlatformPiece(_platformPieceAddress);

        if (platformPiece == null) return;

        ClearPlatformPieces();

        var pieceXSize = platformPiece.Size.x;

        int pieceCount = Mathf.CeilToInt(fullPlatformSize / pieceXSize);

        Vector2 basePosition = new(transform.position.x, transform.position.y - PLATFORM_PIECE_Y_POSITION_OFFSET);

        var centerPiece = Instantiate(platformPiece, basePosition, Quaternion.identity, transform);
        _platformPieces.Enqueue(centerPiece);

        for (int i = 1; i <= pieceCount / 2; i++)
        {
            Vector2 leftPosition = basePosition - new Vector2(i * pieceXSize, 0);
            var leftPiece = Instantiate(platformPiece, leftPosition, Quaternion.identity, transform);
            _platformPieces.Enqueue(leftPiece);

            Vector2 rightPosition = basePosition + new Vector2(i * pieceXSize, 0);
            var rightPiece = Instantiate(platformPiece, rightPosition, Quaternion.identity, transform);
            _platformPieces.Enqueue(rightPiece);
        }
    }

    private void ClearPlatformPieces()
    {
        foreach (var platformPieces in _platformPieces)
        {
            Destroy(platformPieces.gameObject);
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

    private void PlayerRespawn()
    {
        PlatformActivation();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<CharacterController>(out var player))
        {
            // Останавливаем процесс старта следующего уровня, если игрок вернулся на платформу
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                Debug.Log("Игрок вернулся на платформу, отменяем запуск следующего уровня.");
            }

            PlayerSaving();
        }
    }

    private void PlayerSaving()
    {
        // _saveStatus = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<CharacterController>(out var player))
        {
            // Запускаем процесс старта следующего уровня, если игрок ушел с платформы
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }

            _cancellationTokenSource = new CancellationTokenSource();
            NextLevelStart(_cancellationTokenSource.Token).Forget();
        }
    }

    private async UniTask NextLevelStart(CancellationToken cancellationToken)
    {
        try
        {
            Debug.Log("Ожидание запуска следующего уровня...");
            await UniTask.Delay(System.TimeSpan.FromSeconds(_waitTimeForStartNewLevel), cancellationToken: cancellationToken);
            PlatformDeactivation();
            Debug.Log("Запускаем следующий уровень.");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Запуск следующего уровня отменён.");
        }
    }

    private void PlatformDeactivation()
    {
        gameObject.SetActive(false);
    }

    private void PlatformActivation()
    {
        gameObject.SetActive(true);

    }

    private void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
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

}

