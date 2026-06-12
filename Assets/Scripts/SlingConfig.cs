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
    [Range(0f, 1f)] public float aimTimeScale = 0.3f;
    public float circleCastRadius = 0.4f;
    public float minDistance = 2f;
    public int maxBounces = 4;
    public KickConfig kick = new KickConfig { angle = 45f, speed = 50f };
}
