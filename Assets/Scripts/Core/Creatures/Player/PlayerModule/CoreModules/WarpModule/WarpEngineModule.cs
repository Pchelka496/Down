using Additional;
using Creatures.Player;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

/// <summary>
/// Warp engine module for controlling character movement.
/// </summary>
/// <remarks>
/// Warp Drive is a hypothetical technology that allows you to travel faster than the speed of light, 
/// by warping space-time around an object. In science fiction, such as in the universes 
/// Star Trek or Warhammer 40k, warp drive allows you to travel 
/// travel through hyperspace, circumventing the laws of classical physics.
/// 
/// In the context of this class, warp drive symbolizes the ability of an object (e.g., the player) to 
/// to quickly travel significant distances or change altitude using smooth motion 
/// with a controlled speed schedule.
/// 
/// Interesting fact: the idea of warp drive has a scientific basis, based on the 
/// solutions to Einstein's equations in the general theory of relativity, such as the concept of the Alcubierre warp bubble. 
/// However, none of these solutions have yet been practically realized in the real world. https://www.youtube.com/watch?v=IxYEtS1pakQ&t=1s
/// </remarks>
public class WarpEngineModule : BaseModule
{
    [SerializeField] Transform _player;
    [SerializeField] WarpEngineVisualPart _visualPart;
    [SerializeField] AnimationCurve _speedCurve;

    CancellationTokenSource _movingCts;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct()
    {
        _visualPart.Initialize();
    }

    public async UniTask StartMoving(float targetHeight, float moveDuration)
    {
        ClearToken();
        _movingCts = new();

        _visualPart.UpdateMoveDuration(moveDuration);
        _visualPart.Play();

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
