using System;
using UnityEngine;

public class LevelBehaviour : MonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;
    public Vector3 SpawnPoint => _spawnPoint.position;
    [SerializeField] private Transform _goalPoint;
    [SerializeField] private Collider2D _boundingShape;
    public Collider2D BoundingShape => _boundingShape;

    private PlayerBehaviour _player;
    public PlayerBehaviour Player => _player;

    private EnemyBehaviour[] _enemies;

    private Action _onGoalReached;
    private bool _goalReached;

    public void Initialize(PlayerBehaviour playerPrefab, CameraController cameraController, IPointerInput pointerInput, Action<int> onEnemyDamaged, Action onEnemyDead, Action onGoalReached)
    {
        _onGoalReached = onGoalReached;
        _goalReached = false;

        _player = Instantiate(playerPrefab, _spawnPoint.position, Quaternion.identity);
        _player.Initialize(cameraController, pointerInput, null, null);
        _enemies = GetComponentsInChildren<EnemyBehaviour>();
        foreach (var enemy in _enemies)
            enemy.Initialize(_player, onEnemyDamaged, onEnemyDead);
    }

    public void FixedTick(float fixedDeltaTime)
    {
        _player?.FixedTick(fixedDeltaTime);
        foreach (var enemy in _enemies)
            enemy.FixedTick(fixedDeltaTime);

        if (!_goalReached && _player != null && _player.transform.position.y >= _goalPoint.position.y)
        {
            _goalReached = true;
            _onGoalReached?.Invoke();
        }
    }

    public void Tick(float deltaTime)
    {
        _player?.Tick(deltaTime);
        foreach (var enemy in _enemies)
            enemy.Tick(deltaTime);

        if (!_goalReached && _player != null && _player.transform.position.y >= _goalPoint.position.y)
        {
            _goalReached = true;
            _onGoalReached?.Invoke();
        }
    }
}
