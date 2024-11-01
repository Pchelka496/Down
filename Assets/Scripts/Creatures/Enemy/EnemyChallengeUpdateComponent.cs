using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;
using System;

public class EnemyChallengeUpdateComponent : MonoBehaviour
{
    const float UPDATE_CHALLENGE_DELAY = 3f;
    [SerializeField] EnemyVisualPart _visualPart;
    bool _isSeen;
    EnemyManager _enemyManager;

    CancellationTokenSource _cancellationTokenSource;

    [Inject]
    private void Construct(EnemyManager enemyManager)
    {
        _enemyManager = enemyManager;
    }

    private void OnBecameVisible()
    {
        _isSeen = true;

        ClearToken(ref _cancellationTokenSource);
    }

    private void OnBecameInvisible()
    {
        if (_isSeen)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            DelayChallengeUpdate(_cancellationTokenSource.Token).Forget();
        }
    }

    private async UniTaskVoid DelayChallengeUpdate(CancellationToken cancellationToken)
    {
        try
        {
            await UniTask.WaitForSeconds(UPDATE_CHALLENGE_DELAY, cancellationToken: cancellationToken);

            if (_isSeen && !cancellationToken.IsCancellationRequested)
            {
                _enemyManager.UpdateChallenge(_visualPart);
            }
        }
        catch (OperationCanceledException)
        {
            _isSeen = false;
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
