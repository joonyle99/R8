using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private CameraController _cameraController;
    private IPointerInput _pointerInput;

    [SerializeField] private LevelData[] _levels;
    [SerializeField] private Transform _root;
    [SerializeField] private PlayerBehaviour _playerPrefab;

    private int _index;
    private LevelBehaviour _currLv;

    public void Initialize(CameraController cameraController, IPointerInput pointerInput)
    {
        _cameraController = cameraController;
        _pointerInput = pointerInput;

        LoadNext();
    }

    public void LoadNext()
    {
        if (_levels == null || _levels.Length == 0) return;
        if (_currLv != null) Destroy(_currLv.gameObject);
        var levelData = _levels[_index % _levels.Length];
        if (levelData == null) return;

        _currLv = Instantiate(levelData.levelPrefab, _root);
        _currLv.Initialize(_playerPrefab, _cameraController, _pointerInput, null, null, LoadNext);

        _index++;
    }

    public void FixedTick(float deltaTime)
    {
        _currLv?.FixedTick(deltaTime);
    }

    public void Tick(float deltaTime)
    {
        _currLv?.Tick(deltaTime);
    }
}
