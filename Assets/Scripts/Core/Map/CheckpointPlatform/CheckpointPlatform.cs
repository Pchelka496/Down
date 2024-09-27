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
    const float PLATFORM_PIECE_Y_POSITION_OFFSET = 2f;

    [SerializeField] float _waitTimeForStartNewLevel;
    [SerializeField] string _platformPieceAddress;
    float _levelWidth;
    MapController _mapController;
    Queue<PlatformPiece> _platformPieces;
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

    public void Initialize(float height)
    { 
    
    }

    private async UniTask ResizeAsync(float fullPlatformSize)
    {
        var platformPiece = await LoadPlatformPiece(_platformPieceAddress);

        if (platformPiece == null) return;

        ClearPlatformPieces();

        var pieceXSize = platformPiece.Size.x;

        int pieceCount = Mathf.CeilToInt(fullPlatformSize / pieceXSize);

        Vector2 basePosition = transform.position;

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
            Destroy(platformPieces);
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
                Debug.Log("The platform has been successfully uploaded.");
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

}

