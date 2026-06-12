using UnityEngine;

public class PlatformerSensor : MonoBehaviour
{
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _platformLayer;
    [SerializeField] private float _castDistance = 0.1f;

    public LayerMask GroundLayer => _groundLayer;
    public LayerMask PlatformLayer => _platformLayer;
    public LayerMask GroundAndPlatform => _groundLayer | _platformLayer;

    public bool IsGrounded { get; private set; }

    private CircleCollider2D _circleCollider;
    private Rigidbody2D _rigid;

    public void Initialize()
    {
        _circleCollider = GetComponent<CircleCollider2D>();
        _rigid = GetComponent<Rigidbody2D>();
    }

    public void Tick()
    {
        var center = (Vector2)transform.position + _circleCollider.offset;
        var radius = _circleCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);

        // 상승 중이면 착지 판정 불가 (플랫폼 아래서 위로 통과 시 오감지 방지)
        if (_rigid.linearVelocity.y > 0f)
        {
            IsGrounded = false;
            return;
        }

        // 아래로 캐스트: solid 바닥 + 플랫폼 모두 감지
        IsGrounded = CircleCast(center, radius, Vector2.down, GroundAndPlatform);
    }

    private bool CircleCast(Vector2 origin, float radius, Vector2 direction, LayerMask mask)
    {
        return Physics2D.CircleCast(origin, radius, direction, _castDistance, mask);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_circleCollider == null) return;

        var center = (Vector2)transform.position + _circleCollider.offset;
        var radius = _circleCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);

        DrawCastGizmo(center, radius, Vector2.down, GroundAndPlatform);
    }

    private void DrawCastGizmo(Vector2 center, float radius, Vector2 direction, LayerMask mask)
    {
        bool hit = Physics2D.CircleCast(center, radius, direction, _castDistance, mask);
        Gizmos.color = hit ? Color.green : Color.red;
        Gizmos.DrawWireSphere(center + direction * _castDistance, radius);
    }
#endif
}
