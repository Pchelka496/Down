using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class PlayerInputController //: IInputController
{
    readonly Controls _controls;

    //public Vector2 InputMoveValue
    //{
    //    get
    //    {
    //        Vector2 moveValue = _controls.Player.MoveDirection.ReadValue<Vector2>();
    //        return _invertFlag ? new Vector2(-moveValue.x, moveValue.y) : moveValue;
    //    }
    //}

    //public PlayerInputController(Controls controls)
    //{
    //    _controls = controls;
    //    _controls.Enable();
    //    _ = Jump();
    //}

    //public PlayerInputController(Controls controls, Action jumpEvent) : this(controls)
    //{
    //    JumpAction = jumpEvent;
    //    SubscribeToJumpEvent(jumpEvent);
    //}

    //public void InvertValue(bool flag) => _invertFlag = flag;

    //private async UniTask Jump()
    //{
    //    while (true)
    //    {
    //        await UniTask.WaitForSeconds(JUMP_COOLDOWN);

    //        await UniTask.WaitUntil(() => _controls.Player.MoveDirection.ReadValue<Vector2>().y >= 0.9f);
    //        await UniTask.SwitchToMainThread();
    //        JumpAction?.Invoke();
    //        await UniTask.WaitUntil(() => _controls.Player.MoveDirection.ReadValue<Vector2>().y < 0.9f);
    //    }
    //}

    //public void SubscribeToJumpEvent(Action action) => JumpAction += action;
    //public void UnsubscribeToJumpEvent(Action action) => JumpAction -= action;

}
