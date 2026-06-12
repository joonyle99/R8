using UnityEngine;
using JoonyleGameDevKit;

/// <summary>
/// 공중 상태 (구 Boost + Fall 통합).
/// 발사 직후에는 비행 시간 예산이 있어 예산 바운스가 가능하고,
/// 예산이 끝나면(또는 발사 없이 떨어지면) 보너스 킥 1회만 남는다.
/// 벽/바닥 판정은 전부 SlingSimulation.Tick()이 처리한다.
/// </summary>
public sealed class PlayerAirState : StateBase<PlayerBehaviour>
{
    private Rigidbody2D _rigid;
    private SlingConfig _config;
    private SlingState _simulation;
    private bool _isHitPausing;

    public override void Enter(PlayerBehaviour owner)
    {
        // owner.PlayPlayerAnimation(PlayerAnimationState.Roll);

        _isHitPausing = false;

        var sling = owner.SlingBehaviour;
        _rigid  = sling.Rigid;
        _config = sling.Config;

        if (sling.ConsumeSling())
        {
            // 발사 직후: 첫 구간 방향 = Points[0] → Points[1]
            var shot = sling.LastSlingResult;
            var dir = (shot.Points[1] - shot.Points[0]).normalized;
            owner.SetFacingDir(dir.x > 0f);
            _simulation = SlingState.Create(_rigid.position, dir, _config);

            _isHitPausing = true;
            owner.SquashStretch?.HoldLand(); // 예비동작: 웅크린 채 유지
            owner.PauseAndLaunch(owner.LaunchHitPauseDuration, _simulation.Velocity, () =>
            {
                _isHitPausing = false;
                owner.SquashStretch?.OnLaunch();
            });
        }
        else
        {
            _simulation = default;
            _simulation.Velocity = _rigid.linearVelocity;
        }
    }

    public override void Exit(PlayerBehaviour owner) { }

    public override void FixedUpdate(PlayerBehaviour owner, float fixedDeltaTime)
    {
        if (_isHitPausing) return;
        if (_simulation.Velocity.sqrMagnitude < 0.01f) return;

        _simulation.Position = _rigid.position;

        var evt = SlingSimulator.Tick(ref _simulation, _config, owner.PlatformerSensor.GroundLayer, fixedDeltaTime);

        _rigid.linearVelocity = _simulation.Velocity;

        switch (evt)
        {
            case SlingEvent.Bounce:
            {
                _rigid.position = _simulation.Position;
                var vel = _simulation.Velocity;
                var isFacingRight = vel.x > 0f;
                _isHitPausing = true;
                owner.SetFacingDir(isFacingRight);
                owner.SquashStretch?.SetContactDir(ContactDir.Left);
                owner.PlayPlayerAnimation(PlayerAnimationState.Wall);
                owner.SquashStretch?.HoldWallHit();
                owner.PauseAndLaunch(owner.BounceHitPauseDuration, vel, () =>
                {
                    _isHitPausing = false;
                    owner.SquashStretch?.OnLaunch();
                    owner.PlayPlayerAnimation(PlayerAnimationState.Roll);
                });
                break;
            }
        }
    }

    public override void Update(PlayerBehaviour owner, float deltaTime)
    {
        if (_isHitPausing) return;
        if (owner.PlatformerSensor.IsGrounded)
            owner.ChangeState<PlayerGroundState>();
    }
}
