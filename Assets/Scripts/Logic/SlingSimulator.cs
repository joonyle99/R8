using UnityEngine;

public enum SlingEvent
{
    None,
    Bounce,
}

public struct SlingState
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Remaining;
    public float Total; // 누적 이동 거리
    public int Bounces;

    public static SlingState Create(Vector2 origin, Vector2 dir, SlingConfig config)
    {
        return new SlingState
        {
            Position = origin,
            Velocity = dir * config.slingSpeed,
            Remaining = config.slingTime,
        };
    }
}

/// <summary>
/// 조준선 예측(TrajectorySolver)과 실제 공중 이동(PlayerAirState)이 공유하는 한 스텝 시뮬레이션.
/// 규칙(중력, 캐스트, 바닥/벽 분류, 거리 예산, 킥 방향, 보너스 바운스)은 전부 여기에만 존재한다.
/// 호출자가 이동을 직접 책임지는 경우(리지드바디)에는 Tick 전에 Position/Velocity를 동기화하고,
/// 반환된 이벤트에 따라 Velocity만 가져다 쓰면 된다.
/// </summary>
public static class SlingSimulator
{
    public static SlingEvent Tick(ref SlingState state, SlingConfig config, LayerMask groundLayer, float dt)
    {
        state.Velocity.y -= config.slingGravity * dt;

        state.Remaining -= dt;

        var step = state.Velocity * dt;
        var stepDist = step.magnitude;

        var allHits = Physics2D.CircleCastAll(state.Position, config.circleCastRadius, step.normalized, stepDist, groundLayer);
        foreach (var h in allHits)
        {
            if (h.distance <= 0f) continue;

            state.Total += h.distance;
            state.Position = h.centroid;

            if (Vector2.Angle(h.normal, Vector2.up) <= config.wallAngleThreshold)
                continue;

            if (state.Total >= config.minDistance && state.Remaining > 0f && state.Bounces < config.maxBounces)
            {
                state.Bounces++;
                state.Velocity = Kick(h.normal, config.kick);
                return SlingEvent.Bounce;
            }

            return SlingEvent.None;
        }

        state.Position += step;
        state.Total += stepDist;
        return SlingEvent.None;
    }

    // 벽 킥 속도: angle 0° = 벽 반대편 수평, 90° = 수직 위
    // (wallNormalThreshold가 벽을 거의 수직면으로 보장하므로 normal.x 부호만으로 좌우를 정한다)
    public static Vector2 Kick(Vector2 normal, SlingConfig.KickConfig kick)
    {
        var rad = kick.angle * Mathf.Deg2Rad;
        var away = Mathf.Sign(normal.x);
        return new Vector2(Mathf.Cos(rad) * away, Mathf.Sin(rad)) * kick.speed;
    }
}
