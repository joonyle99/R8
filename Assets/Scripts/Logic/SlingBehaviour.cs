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

    public SlingResult LastSlingResult { get; private set; }

    private bool _isPendingSling; // Shoot 직후 한 번만 true — AirState가 "발사 진입"인지 "그냥 낙하"인지 구분하는 용도

    public void Initialize(Rigidbody2D rigid, LayerMask groundLayer)
    {
        _rigid = rigid;
        
        _solver = new TrajectorySolver(_config, groundLayer);

        _line = GetComponentInChildren<LineRenderer>();
        _line.textureMode = LineTextureMode.Tile;
        _line.numCornerVertices = 90;
        _line.useWorldSpace = true;
        _line.enabled = false;

        _bounceMarkers = new SpriteRenderer[_config.maxBounces];
        for (int i = 0; i < _bounceMarkers.Length; i++)
        {
            _bounceMarkers[i] = Instantiate(_bounceMarkerPrefab, transform);
            _bounceMarkers[i].enabled = false;
        }
    }

    public void SetActiveSling(bool active) => _isActiveSling = active;

    // ============ ... ============

    public void ShowTrajectory(Vector2 dragOffset)
    {
        var slingDir = (-1) * dragOffset.normalized;
        var slingResult = _solver.Solve(_rigid.position, slingDir);

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

    public void HideTrajectory()
    {
        _line.enabled = false;
        foreach (var marker in _bounceMarkers)
            marker.enabled = false;
    }

    public void SlingShoot(Vector2 dragOffset)
    {
        var slingDir = (-1) * dragOffset.normalized;
        LastSlingResult = _solver.Solve(_rigid.position, slingDir);
        _isPendingSling = true;
    }
    public bool ConsumeSling()
    {
        if (!_isPendingSling) return false;
        _isPendingSling = false;
        return true;
    }
}
