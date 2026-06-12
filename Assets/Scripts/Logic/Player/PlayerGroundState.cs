using UnityEngine;
using JoonyleGameDevKit;

public sealed class PlayerGroundState : StateBase<PlayerBehaviour>
{
    public override void Enter(PlayerBehaviour owner)
    {
        owner.SquashStretch?.SetContactDir(ContactDir.Down);
        owner.PlayPlayerAnimation(PlayerAnimationState.Idle);

        var velocity = owner.Rigid.linearVelocity;
        velocity.x = 0f;
        owner.Rigid.linearVelocity = velocity;
    }

    public override void Exit(PlayerBehaviour owner) { }

    public override void Update(PlayerBehaviour owner, float deltaTime)
    {
        if (!owner.PlatformerSensor.IsGrounded)
        {
            owner.ChangeState<PlayerAirState>();
        }
        else if (owner.PointerInput.IsDragging)
        {
            owner.ChangeState<PlayerAimState>();
        }
    }
}
