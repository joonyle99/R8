using UnityEngine;

[CreateAssetMenu(fileName = "SlingConfig", menuName = "Rising Hook/SlingConfig")]
public class SlingConfig : ScriptableObject
{
    [System.Serializable]
    public struct KickConfig
    {
        [Range(0f, 90f)] public float angle; // 벽 킥 각도: 0 = 벽 반대편 수평, 90 = 수직 위
        public float speed;
    }

    public float slingGravity = 30f;
    public float slingSpeed = 20f;
    public float slingTime = 0.3f;
    public float bonusTime = 0.2f;
    [Range(0f, 90f)] public float wallAngleThreshold = 10f; // 법선이 수평에서 이 각도 이내면 벽
    [Range(-90f, 90f)] public float groundShotMaxAngle = 10f; // 조준 고도각(수평 기준)이 이 값 이하면 땅샷 (포물선 대신 직선 발사)
    public float groundShotAimLineLength = 3f; // 땅샷 조준선 길이
    public float aimTimeScale = 0.15f;
    public float circleCastRadius = 0.4f;
    public float minDistance = 2f;
    public int maxBounces = 4;
    public int maxSlingCharges = 3; // 공중 연속 발사 가능 횟수 (착지 시 풀회복, 적 처치 시 +1)
    public KickConfig kick = new KickConfig { angle = 45f, speed = 50f };
}
