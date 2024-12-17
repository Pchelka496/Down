using Creatures.Player;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class PlayerPositionMeter
{
    Transform _playerTransform;

    public static float YPosition { get; private set; }
    public static float XPosition { get; private set; }

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(PlayerController player)
    {
        _playerTransform = player.transform;
        UpdatePlayerPosition().Forget();
    }

    private async UniTaskVoid UpdatePlayerPosition()
    {
        while (true)
        {
            YPosition = _playerTransform.position.y;
            XPosition = _playerTransform.position.x;

            await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate);
        }
    }
}
