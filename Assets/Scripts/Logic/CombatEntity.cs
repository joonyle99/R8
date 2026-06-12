using System;
using UnityEngine;

public abstract class CombatEntity : MonoBehaviour
{
    public Animator Animator { get; protected set; }
    public Transform Pivot { get; protected set; }
    public SpriteRenderer SpriteRenderer { get; protected set; }
    protected Material Material { get; set; }
    public Rigidbody2D Rigid { get; protected set; }

    [SerializeField] protected int maxHp;
    public int MaxHp => maxHp;
    [SerializeField] protected int attack;
    public int Attack => attack;

    protected int currHp;
    public int CurrHp => currHp;

    public bool IsDead => currHp <= 0;
    public bool IsInvincible { get; set; }
    public bool CanAttack { get; set; } = true;

    private Action<int> _onDamaged;
    private Action _onDead;
    
    protected bool isFacingRight;

    protected void InitCombatEntity(Action<int> onDamaged, Action onDead)
    {
        _onDamaged = onDamaged;
        _onDead = onDead;

        Animator = GetComponentInChildren<Animator>();
        Pivot = transform.Find("Visual/Pivot");
        SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        Material = SpriteRenderer.material;
        Rigid = GetComponentInChildren<Rigidbody2D>();

        currHp = maxHp;
        isFacingRight = true;
    }

    public virtual void FixedTick(float deltaTime)
    {
        
    }

    public virtual void Tick(float deltaTime)
    {
        
    }

    // ========= ... =========

    private void OnTriggerEnter2D(Collider2D collider)
    {
        HandleTriggerEnter(collider);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        HandleTriggerStay(collider);
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        HandleTriggerExit(collider);
    }

    // ========= ... =========

    protected virtual void HandleTriggerEnter(Collider2D collider)
    {
        if (collider.TryGetComponent<CombatEntity>(out var combat))
        {
            if (!combat.IsDead && CanAttack)
            {
                combat.TakeDamage(attack, Rigid.position);
                OnHit(combat);
                if (combat.IsDead)
                    OnKill(combat);
            }
        }
    }

    protected virtual void HandleTriggerStay(Collider2D collider)
    {
        // do nothing
    }

    protected virtual void HandleTriggerExit(Collider2D collider)
    {
        // do nothing
    }

    // ========= ... =========

    protected virtual void OnDamaged(int damage, Vector2 sourcePos)
    {
        _onDamaged?.Invoke(damage);
    }

    protected virtual void OnDead()
    {
        _onDead?.Invoke();

        Destroy(gameObject);
    }

    protected virtual void OnHit(CombatEntity target)
    {
        // do nothing
    }
    protected virtual void OnKill(CombatEntity target)
    {
        // do nothing
    }

    // ========= ... =========

    public virtual void TakeDamage(int damage, Vector2 sourcePos)
    {
        if (IsDead || IsInvincible) return;
        currHp -= damage;
        OnDamaged(damage, sourcePos);
        if (IsDead) OnDead();
    }

    // ========= ... =========
    
    public void PlayAnimation(int stateHash, float crossFade = 0f) => Animator.CrossFade(stateHash, crossFade);
    public void PlayAnimation(string stateName, float crossFade = 0f) => Animator.CrossFade(stateName, crossFade);

    // ========= ... =========

    public void SetFacingDir(bool isFacingRight)
    {
        this.isFacingRight = isFacingRight;

        var scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (this.isFacingRight ? 1f : -1f);
        transform.localScale = scale;
    }
}
