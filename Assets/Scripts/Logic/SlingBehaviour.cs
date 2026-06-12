using System;
using UnityEngine;

public class SlingBehaviour : MonoBehaviour
{
    [SerializeField] private SlingConfig _config;
    public SlingConfig Config => _config;

    [SerializeField] private SpriteRenderer _bounceMarkerPrefab;

    private Rigidbody2D _rigid;
    public Rigidbody2D Rigid => _rigid;
    private TrajectorySolver _solver;
    private LineRenderer _line;
    private SpriteRenderer[] _bounceMarkers;

    private bool _isActiveSling;
    public bool IsActiveSling => _isActiveSling;

    private bool _isPendingShot; // Shoot 직후 한 번만 true — AirState가 "발사 진입"인지 "그냥 낙하"인지 구분하는 용도
    public Vector2 LastShotDir { get; private set; }

    public int MaxSlingCharges => _config.maxSlingCharges;
    public int CurrSlingCharges { get; private set; }
    public bool HasSlingCharge => CurrSlingCharges > 0;
    public event Action<int, int> OnSlingChargesChanged; // (curr, max)

    public void Initialize(Rigidbody2D rigid, LayerMask groundLayer)
    {
        _rigid = rigid;

        _solver = new TrajectorySolver(_config, groundLayer);

        _line = GetComponentInChildren<LineRenderer>();
        _line.textureMode = LineTextureMode.Tile;
        _line.useWorldSpace = true;
        _line.enabled = false;

        _bounceMarkers = new SpriteRenderer[_config.maxBounces];
        for (int i = 0; i < _bounceMarkers.Length; i++)
        {
            _bounceMarkers[i] = Instantiate(_bounceMarkerPrefab, transform);
            _bounceMarkers[i].enabled = false;
        }

        CurrSlingCharges = _config.maxSlingCharges;
    }

    public void SetActiveSling(bool active) => _isActiveSling = active;

    // ============ ... ============

    public void ShowTrajectory(Vector2 dragOffset)
    {
        var slingDir = (-1) * dragOffset.normalized;

        // 조준선 원점은 물리 위치(_rigid.position)가 아니라 보간된 렌더 위치를 쓴다.
        // 물리 위치는 물리 스텝에서만 갱신돼서, 슬로우(aimTimeScale) 중 낙하하며 조준하면 선이 한 박자 늦게 따라온다.
        var origin = (Vector2)_rigid.transform.position;

        // 잠시 주석처리
        // if (SlingSimulator.IsGroundShot(slingDir, _config))
        // {
        //     ShowGroundShotLine(slingDir);
        //     return;
        // }

        var slingResult = _solver.Solve(origin, slingDir);

        {
            _line.positionCount = slingResult.Points.Count;
            for (int i = 0; i < slingResult.Points.Count; i++)
                _line.SetPosition(i, slingResult.Points[i]);
            _line.enabled = true;
        }

        {
            for (int i = 0; i < _bounceMarkers.Length; i++)
            {
                if (i < slingResult.BouncePoints.Count)
                {
                    _bounceMarkers[i].transform.position = slingResult.BouncePoints[i];
                    _bounceMarkers[i].enabled = true;
                }
                else
                {
                    _bounceMarkers[i].enabled = false;
                }
            }
        }
    }

    // 땅샷 조준선: 포물선 대신 짧은 고정 길이 직선
    private void ShowGroundShotLine(Vector2 slingDir)
    {
        var origin = (Vector2)_rigid.transform.position;

        _line.positionCount = 2;
        _line.SetPosition(0, origin);
        _line.SetPosition(1, origin + slingDir * _config.groundShotAimLineLength);
        _line.enabled = true;

        foreach (var marker in _bounceMarkers)
            marker.enabled = false;
    }

    public void HideTrajectory()
    {
        _line.enabled = false;
        foreach (var marker in _bounceMarkers)
            marker.enabled = false;
    }

    public void ShootSling(Vector2 dragOffset)
    {
        _isPendingShot = true;

        var shotDir = (-1) * dragOffset.normalized;
        LastShotDir = shotDir;

        CurrSlingCharges = Mathf.Max(0, CurrSlingCharges - 1);
        OnSlingChargesChanged?.Invoke(CurrSlingCharges, MaxSlingCharges);
    }

    public void RestoreCharges()
    {
        if (CurrSlingCharges == MaxSlingCharges) return;

        CurrSlingCharges = MaxSlingCharges;
        OnSlingChargesChanged?.Invoke(CurrSlingCharges, MaxSlingCharges);
    }

    public void AddSlingCharge(int amount = 1)
    {
        if (CurrSlingCharges >= MaxSlingCharges) return;

        CurrSlingCharges = Mathf.Min(MaxSlingCharges, CurrSlingCharges + amount);
        OnSlingChargesChanged?.Invoke(CurrSlingCharges, MaxSlingCharges);
    }

    public bool ConsumeSling()
    {
        if (!_isPendingShot) return false;
        _isPendingShot = false;
        return true;
    }
}
