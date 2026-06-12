using System;
using UnityEngine;
using DG.Tweening;
using JoonyleGameDevKit;
using System.Collections;
using System.Collections.Generic;

public enum PlayerAnimationState
{
    Idle,
    Roll,
    Wall,
}

public sealed class PlayerBehaviour : SlingEntity
{
    public CameraController CameraController { get; private set; }
    public IPointerInput PointerInput { get; private set; }

    public float Gravity => SlingBehaviour.Config.slingGravity;

    [Space]

    [SerializeField] private float _launchHitPauseDuration = 0.06f;
    [SerializeField] private float _bounceHitPauseDuration = 0.05f;
    public float LaunchHitPauseDuration => _launchHitPauseDuration;
    public float BounceHitPauseDuration => _bounceHitPauseDuration;

    [SerializeField] private float _knockbackX = 12f;
    [SerializeField] private float _knockbackY = 8f;
    [SerializeField] private float _invincibleDuration = 1.5f;
    [SerializeField] private float _blinkInterval = 0.08f;
    [SerializeField] private float _hitStopDuration = 0.1f;
    [SerializeField] private float _hitStopTimeScale = 0.1f;

    private Coroutine _invincibleCoroutine;

    private PlatformerSensor _platformerSensor;
    public PlatformerSensor PlatformerSensor => _platformerSensor;

    private SquashStretch _squashStretch;
    public SquashStretch SquashStretch => _squashStretch;
    
    private StateMachine<PlayerBehaviour> _fsm;
    public StateMachine<PlayerBehaviour> FSM => _fsm;

#if UNITY_EDITOR
    private string _debugState;
#endif

    private static readonly int IDLE = Animator.StringToHash("Idle");
    private static readonly int ROLL = Animator.StringToHash("Roll");
    private static readonly int WALL = Animator.StringToHash("Wall");
    
    private static readonly float HIT_COLOR_MIN = 0f;
    private static readonly float HIT_COLOR_MAX = 0.7f;
    private static readonly float HIT_DURATION = 0.2f;

    private Sequence _hitColorSequence;

    public void Initialize(CameraController cameraController, IPointerInput pointerInput, Action<int> onDamaged, Action onDead)
    {
        _platformerSensor = GetComponent<PlatformerSensor>();
        _platformerSensor.Initialize();

        InitSlingEntity(onDamaged, onDead, _platformerSensor.GroundLayer);

        CameraController = cameraController;
        CameraController.ActivateFollow(transform);
        PointerInput = pointerInput;

        CanAttack = false;
        
        _squashStretch = GetComponent<SquashStretch>();
        _squashStretch.Initialize(Pivot, Animator.transform);

        _fsm = new StateMachine<PlayerBehaviour>(this);
        _fsm.AddState(new PlayerGroundState());
        _fsm.AddState(new PlayerAimState());
        _fsm.AddState(new PlayerAirState());

        ChangeState<PlayerAirState>();
    }

    // ============ ... ============

    public override void FixedTick(float fixedDeltaTime)
    {
        if (IsDead) return;

        base.FixedTick(fixedDeltaTime);

        _platformerSensor?.FixedTick(fixedDeltaTime);

        if (_platformerSensor != null && !_platformerSensor.IsGrounded)
            ApplyGravity(fixedDeltaTime);

        _fsm?.FixedUpdate(fixedDeltaTime);
    }

    public override void Tick(float deltaTime)
    {
        if (IsDead) return;

        base.Tick(deltaTime);

        _fsm?.Update(deltaTime);
    }

    public void ChangeState<TState>() where TState : StateBase<PlayerBehaviour>
    {
#if UNITY_EDITOR
    _debugState = typeof(TState).Name;
#endif

        _fsm.ChangeState<TState>();
    }

    // ============ ... ============

    protected override void OnDamaged(int damage, Vector2 sourcePos)
    {
        base.OnDamaged(damage, sourcePos);

        var dirX = Rigid.position.x >= sourcePos.x ? 1f : -1f;
        Rigid.linearVelocity = new Vector2(dirX * _knockbackX, _knockbackY);

        if (_invincibleCoroutine != null) StopCoroutine(_invincibleCoroutine);
        _invincibleCoroutine = StartCoroutine(InvincibleRoutine());
    }

    protected override void OnHit(CombatEntity target)
    {
        base.OnHit(target);

        // StartCoroutine(HitStop());
    }

    protected override void OnKill(CombatEntity target)
    {
        base.OnKill(target);

        SlingBehaviour.AddSlingCharge(); // 적 처치 시 차지 +1

        if (!_platformerSensor.IsGrounded)
            ChangeState<PlayerAimState>();
    }

    // ========= ... =========

    private IEnumerator InvincibleRoutine()
    {
        IsInvincible = true;

        var blink = SpriteRenderer.DOFade(0f, _blinkInterval)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.Linear)
            .SetUpdate(true)
            .SetLink(gameObject);

        yield return new WaitForSecondsRealtime(_invincibleDuration);

        blink.Kill();
        var color = SpriteRenderer.color;
        color.a = 1f;
        SpriteRenderer.color = color;

        IsInvincible = false;
        _invincibleCoroutine = null;
    }

    private IEnumerator HitStop()
    {
        Time.timeScale = _hitStopTimeScale;
        yield return new WaitForSecondsRealtime(_hitStopDuration);
        Time.timeScale = 1f;
    }

    public void PlayHitEffect()
    {
        _hitColorSequence?.Kill();
        Material.SetFloat("_Amount", HIT_COLOR_MAX);
        var outColor = DOVirtual.Float(HIT_COLOR_MAX, HIT_COLOR_MIN, HIT_DURATION, v => Material.SetFloat("_Amount", v));
        _hitColorSequence = DOTween.Sequence().Append(outColor).OnComplete(() => _hitColorSequence = null);
    }

    // ========= ... =========

    public void PlayPlayerAnimation(PlayerAnimationState state)
    {
        switch (state)
        {
            case PlayerAnimationState.Idle: PlayAnimation(IDLE); break;
            case PlayerAnimationState.Roll: PlayAnimation(ROLL); break;
            case PlayerAnimationState.Wall: PlayAnimation(WALL); break;
        }
    }

    // ========= ... =========

    public void PauseAndLaunch(float duration, Vector2 velocity, Action onResume)
    {
        StartCoroutine(HitPauseRoutine(duration, velocity, onResume));
    }

    private IEnumerator HitPauseRoutine(float duration, Vector2 velocity, Action onResume)
    {
        Rigid.linearVelocity = Vector2.zero;
        Rigid.constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds(duration);
        Rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        Rigid.linearVelocity = velocity;
        onResume?.Invoke();
    }

    // ========= ... =========

    public void ApplyGravity(float deltaTime)
    {
        var vel = Rigid.linearVelocity;
        vel.y -= Gravity * deltaTime;
        Rigid.linearVelocity = vel;
    }

    // ========= ... =========

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsDead) return;

        if (_fsm?.CurrState is PlayerAirState airState)
            airState.OnCollision(this, collision);
    }
}
