using UnityEngine;
using JoonyleGameDevKit;

public sealed class PlayerAimState : StateBase<PlayerBehaviour>
{
    private bool _enteredFromGround;
    private Vector2 _velocityBeforeAim;
    private Vector2 _lastDragOffset;

    public override void Enter(PlayerBehaviour owner)
    {
        Time.timeScale = owner.SlingBehaviour.Config.aimTimeScale;
        owner.SquashStretch?.OnAimStart();

        _enteredFromGround = owner.PlatformerSensor.IsGrounded;
        if (!_enteredFromGround)
        {
            _velocityBeforeAim = owner.Rigid.linearVelocity;
            owner.Rigid.linearVelocity = Vector2.zero;
        }
    }

    public override void Exit(PlayerBehaviour owner)
    {
        owner.SlingBehaviour.HideTrajectory();
        Time.timeScale = 1f;
        owner.SquashStretch?.OnAimEnd();
    }

    public override void Update(PlayerBehaviour owner, float deltaTime)
    {
        var pointerInput = owner.PointerInput;
        var cam = owner.CameraController.MainCamera;
        var worldUnitsPerPixel = cam.orthographicSize * 2f / Screen.height;

        if (pointerInput.IsDragging)
        {
            _lastDragOffset = pointerInput.GetScreenDragDelta * worldUnitsPerPixel;
            if (!Mathf.Approximately(_lastDragOffset.x, 0f))
                owner.SetFacingDir(_lastDragOffset.x < 0f); // 드래그 반대 방향 = 발사 방향
        }

        if (pointerInput.JustReleased)
        {
            if (!pointerInput.JustTapped)
            {
                owner.SlingBehaviour.SlingShoot(_lastDragOffset);
                owner.ChangeState<PlayerAirState>();
            }
            else
            {
                CancelAim(owner);
            }

            return;
        }
        else if (!pointerInput.IsDragging)
        {
            CancelAim(owner);
            owner.SquashStretch?.OnAimEnd();
            return;
        }

        owner.SlingBehaviour.ShowTrajectory(_lastDragOffset);
    }

    private void CancelAim(PlayerBehaviour owner)
    {
        if (_enteredFromGround)
        {
            owner.ChangeState<PlayerGroundState>();
        }
        else
        {
            owner.Rigid.linearVelocity = _velocityBeforeAim;
            owner.ChangeState<PlayerAirState>();
        }
    }
}
