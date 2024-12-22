using Additional;
using Creatures.Player;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class FastTravelModule : BaseModule
{
    [SerializeField] float _preparationForMovingDelay;

    [SerializeField] SoundPlayerRandomPitch _startSound;

    [SerializeField] Transform _player;
    [SerializeField] FastTravelVisualPart _visualPart;
    [SerializeField] AnimationCurve _speedCurve;

    CancellationTokenSource _movingCts;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(AudioSourcePool audioSourcePool)
    {
        _visualPart.Initialize();
        _startSound.Initialize(audioSourcePool);
    }

    public async UniTask PreparationForMoving()
    {
        _startSound.PlayNextSound();
        _visualPart.PlayerPreparation();

        await UniTask.Delay(System.TimeSpan.FromSeconds(_preparationForMovingDelay));
    }

    public async UniTask StartMoving(float targetHeight, float moveDuration)
    {
        ClearToken();
        _movingCts = new();

        _visualPart.UpdateMoveDuration(moveDuration);
        _visualPart.PlayMoving();

        await PlayerMoving(targetHeight, moveDuration, _movingCts.Token);
    }

    private async UniTask PlayerMoving(float targetHeight, float moveDuration, CancellationToken token)
    {
        var elapsedTime = 0f;
        var startPosition = _player.position;
        var targetPosition = new Vector3(startPosition.x, targetHeight, startPosition.z);

        while (elapsedTime < moveDuration && !token.IsCancellationRequested)
        {
            elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp01(elapsedTime / moveDuration);

            var curveValue = _speedCurve.Evaluate(t);
            _player.position = Vector3.Lerp(startPosition, targetPosition, curveValue);

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        if (!token.IsCancellationRequested)
        {
            _player.position = targetPosition;
        }
    }

    public override void DisableModule()
    {
        gameObject.SetActive(false);
    }

    public override void EnableModule()
    {
        gameObject.SetActive(true);
    }

    private void ClearToken() => ClearTokenSupport.ClearToken(ref _movingCts);

    private void OnDestroy()
    {
        ClearToken();
    }
}