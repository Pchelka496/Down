using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;
using System;
using Core.Enemy;

public class EnemyChallengeUpdateComponent : MonoBehaviour
{
    const float UPDATE_CHALLENGE_DELAY = 3f;
    [SerializeField] EnemyVisualPart _visualPart;
    [SerializeField] bool[] _visibilityStatuses;

    bool _isSeen;
    EnemySystemCoordinator _enemyManager;

    CancellationTokenSource _cancellationTokenSource;

#if UNITY_EDITOR
    public bool[] VisibilityStatuses
    {
        get => _visibilityStatuses;
        set => _visibilityStatuses = value;
    }
#endif

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(EnemySystemCoordinator enemyCoordinator)
    {
        _enemyManager = enemyCoordinator;
    }

    private async UniTaskVoid DelayChallengeUpdate(CancellationToken cancellationToken)
    {
        try
        {
            await UniTask.WaitForSeconds(UPDATE_CHALLENGE_DELAY, cancellationToken: cancellationToken);

            if (_isSeen && !cancellationToken.IsCancellationRequested)
            {
                _enemyManager.UpdateChallenge(_visualPart).Forget();
            }
        }
        catch (OperationCanceledException)
        {
            _isSeen = false;
        }
    }

    public void UpdateVisibilityStatus(int index, bool isVisible)
    {
        if (_visibilityStatuses == null || index < 0 || index >= _visibilityStatuses.Length) return;

        _visibilityStatuses[index] = isVisible;
        var allNotVisible = true;

        foreach (var status in _visibilityStatuses)
        {
            if (status)
            {
                allNotVisible = false;
                break;
            }
        }

        if (allNotVisible)
        {
            _isSeen = true;
            ClearToken(ref _cancellationTokenSource);

            _cancellationTokenSource = new();

            DelayChallengeUpdate(_cancellationTokenSource.Token).Forget();
        }
        else
        {
            _isSeen = false;
            ClearToken(ref _cancellationTokenSource);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClearToken(ref CancellationTokenSource cts)
    {
        if (cts == null) return;

        if (!cts.IsCancellationRequested)
        {
            cts.Cancel();
        }

        cts.Dispose();
        cts = null;
    }

    private void OnDestroy()
    {
        ClearToken(ref _cancellationTokenSource);
    }
}
