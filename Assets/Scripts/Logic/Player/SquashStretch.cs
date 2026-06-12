using UnityEngine;
using DG.Tweening;

public enum ContactDir { Left, Down }

public sealed class SquashStretch : MonoBehaviour
{
    [SerializeField] private float _contactOffset = 0.37f;

    [Space]
    
    [SerializeField] private float _duration = 0.2f;
    [SerializeField] private Ease _ease = Ease.OutElastic;

    private Transform _pivot;
    private Transform _animator;
    private Vector3 _baseScale;
    private Tween _tween;

    private void OnDestroy() => _tween?.Kill();

    public void Initialize(Transform pivot, Transform animator)
    {
        _pivot = pivot;
        _animator = animator;
        _baseScale = pivot.localScale;
        _baseScale.x = Mathf.Abs(_baseScale.x); // 좌우 반전은 SetFacingDir가 Visual에서 처리하므로 Pivot 스케일은 항상 양수
    }

    public void OnLaunch() => Play(new Vector3(0.55f, 1.6f, 1f));
    public void OnLand() => Play(new Vector3(1.6f,  0.5f, 1f));
    public void OnWallHit() => Play(new Vector3(0.55f, 1.5f, 1f));

    // 히트포즈 중 유지 (복원 트윈 없음) → OnLaunch로 해제
    public void HoldLand() => Hold(new Vector3(1.6f,  0.5f, 1f));
    public void HoldWallHit() => Hold(new Vector3(0.55f, 1.5f, 1f));

    // 조준 중 유지 → 해제 시 복원
    public void OnAimStart() => Hold(new Vector3(1.2f, 0.75f, 1f));
    public void OnAimEnd() => Restore();

    private void Play(Vector3 ratio)
    {
        if (_pivot == null) return;

        _tween?.Kill();
        _pivot.localScale = Vector3.Scale(_baseScale, ratio);
        _tween = _pivot.DOScale(_baseScale, _duration).SetEase(_ease).SetUpdate(true);
    }

    private void Hold(Vector3 ratio)
    {
        if (_pivot == null) return;

        _tween?.Kill();
        _tween = _pivot.DOScale(Vector3.Scale(_baseScale, ratio), 0.1f).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    private void Restore()
    {
        if (_pivot == null) return;

        _tween?.Kill();
        _tween = _pivot.DOScale(_baseScale, _duration).SetEase(_ease).SetUpdate(true);
    }

    public void SetContactDir(ContactDir dir)
    {
        var snapped = dir switch
        {
            ContactDir.Left => Vector3.left,
            ContactDir.Down => Vector3.down,
            _               => Vector3.zero
        };

        _pivot.localPosition = snapped * _contactOffset;
        _animator.localPosition = -snapped * _contactOffset;
    }
}
