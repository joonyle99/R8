using UnityEngine;
using JoonyleGameDevKit;

public sealed class PlayerAirState : StateBase<PlayerBehaviour>
{
    private Rigidbody2D _rigid;
    private SlingConfig _config;
    private SlingState _budget; // 튕김 자격(누적 거리/남은 시간/횟수) 추적용 — 이동은 rigidbody + ApplyGravity가 담당
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
            var shotDir = sling.LastShotDir;
            owner.SetFacingDir(shotDir.x > 0f);
            _budget = SlingState.Create(_rigid.position, shotDir, _config);

            _isHitPausing = true;

            owner.SquashStretch.HoldSquash(); // 예비동작: 웅크린 채 유지
            owner.PauseAndLaunch(owner.LaunchHitPauseDuration, _budget.Velocity, () =>
            {
                _isHitPausing = false;
                owner.SquashStretch.PlayStretch();
            });
        }
        else
        {
            // 슬링 없이 진입(낙하 등): Remaining 0 → 벽 킥 비활성
            _budget = default;
            _budget.Position = _rigid.position;
        }
    }

    public override void Exit(PlayerBehaviour owner) { }

    public override void FixedUpdate(PlayerBehaviour owner, float fixedDeltaTime)
    {
        if (_isHitPausing) return;

        _budget.Remaining -= fixedDeltaTime;
        _budget.Total += (_rigid.position - _budget.Position).magnitude;
        _budget.Position = _rigid.position;
    }

    public override void Update(PlayerBehaviour owner, float deltaTime)
    {
        if (_isHitPausing) return;

        if (owner.PlatformerSensor.IsGrounded)
        {
            owner.ChangeState<PlayerGroundState>();
        }
        else if (owner.PointerInput.IsDragging && owner.SlingBehaviour.HasSlingCharge)
        {
            // 공중 재조준: 차지가 남아 있을 때만 (0이면 드래그해도 조준 진입 자체가 안 됨)
            owner.ChangeState<PlayerAimState>();
        }
    }

    public void OnCollision(PlayerBehaviour owner, Collision2D collision)
    {
        if (_isHitPausing) return;
        if ((owner.PlatformerSensor.GroundLayer.value & (1 << collision.gameObject.layer)) == 0) return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            var normal = collision.GetContact(i).normal;

            if (!SlingSimulator.IsWall(normal, _config)) continue;
            if (!SlingSimulator.CanBounce(in _budget, _config)) return;

            SlingSimulator.Bounce(ref _budget, normal, _config);

            var vel = _budget.Velocity;

            _isHitPausing = true;

            owner.SetFacingDir(vel.x > 0f);
            owner.SquashStretch.SetContactSurface(ContactSurface.Wall);
            owner.PlayPlayerAnimation(PlayerAnimationState.Wall);
            owner.SquashStretch.HoldSideSquash();
            owner.PauseAndLaunch(owner.BounceHitPauseDuration, vel, () =>
            {
                _isHitPausing = false;
                owner.SquashStretch.PlayStretch();
                owner.PlayPlayerAnimation(PlayerAnimationState.Roll);
            });

            return;
        }
    }
}
