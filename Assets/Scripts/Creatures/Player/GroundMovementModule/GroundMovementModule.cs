using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Zenject;

public class GroundMovementModule : BaseModule, IGroundMovementModule
{
    const float MIN_INTROPALITES_INPUT_VALUE = 0.001f;

    Controls _controls;
    Transform _playerTransform;
    float _maxMoveSpeed = 0.1f;
    Transform _bodySprite;
    float _radius;

    [Inject]
    private void Construct(Controls controls, CharacterController player)
    {
        _controls = controls;
        _bodySprite = player.BodySprite;
        _radius = CharacterController.PLAYER_RADIUS;
        _playerTransform = player.transform;

        transform.parent = player.transform;
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        var rb = player.Rb;
        rb.freezeRotation = false;

        player.GroundMovementModule = this;
    }

    public async UniTask GroundMovement(CancellationTokenSource cts)
    {
        var currentMoveSpeed = 0f;
        var oldLeftInputValue = 0f;
        var oldRightInputValue = 0f;

        while (!cts.IsCancellationRequested)
        {
            var newLeftInput = _controls.Player.MoveLeft.ReadValue<float>();
            var newRightInput = _controls.Player.MoveRight.ReadValue<float>();

            var leftInput = Mathf.Lerp(oldLeftInputValue, newLeftInput, MIN_INTROPALITES_INPUT_VALUE);
            var rightInput = Mathf.Lerp(oldRightInputValue, newRightInput, MIN_INTROPALITES_INPUT_VALUE);

            var moveSpeed = _maxMoveSpeed * (Mathf.Max(newLeftInput, newRightInput) - Mathf.Min(newLeftInput, newRightInput));
            if (newLeftInput > newRightInput)
            {
                moveSpeed = -moveSpeed;
            }

            currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, moveSpeed, 0.1f);

            _playerTransform.Translate(new Vector3(currentMoveSpeed, 0f, 0f));

            var rotationRate = (Mathf.Abs(currentMoveSpeed) / _radius) * Mathf.Rad2Deg;

            if (currentMoveSpeed > 0)
            {
                rotationRate = -rotationRate;
            }

            _bodySprite.Rotate(0, 0, rotationRate);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        }

        await UniTask.CompletedTask;
    }

}
