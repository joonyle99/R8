using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class InGameManager : MonoBehaviour
{
    private PointerInput _pointerInput;
    private PointerInputVisualizer _pointerVisualizer;

    private GameStateController<InGameState> _gameStateController;
    private CameraController _cameraController;
    private UIController _uiController;
    private LevelManager _levelManager;

    // TEMP
    private PlayerBehaviour _player;
    private EnemyBehaviour[] _enemies;

    private void Start()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        _gameStateController.OnStateChanged -= OnStateChanged;
        _gameStateController.OnStateChanged -= _uiController.OnStateChanged;
        _gameStateController.OnStateChanged -= _cameraController.OnStateChanged;
        if (SoundManager.Instance != null) _gameStateController.OnStateChanged -= SoundManager.Instance.OnStateChanged;

        _pointerInput.Dispose();
    }

    private void FixedUpdate()
    {
        var fixedDeltaTime = Time.fixedDeltaTime;

        _levelManager?.FixedTick(fixedDeltaTime);

        // TEMP
        _player?.FixedTick(fixedDeltaTime);
        foreach (var e in _enemies)
            e?.FixedTick(fixedDeltaTime);
    }

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        
        _pointerInput?.Tick(deltaTime);
        _pointerVisualizer?.Tick(deltaTime);
        _levelManager?.Tick(deltaTime);

        // TEMP
        _player?.Tick(deltaTime);
        foreach (var e in _enemies)
            e?.Tick(deltaTime);
    }

    private void Initialize()
    {
        _gameStateController = new GameStateController<InGameState>();
        _cameraController = FindFirstObjectByType<CameraController>();
        _uiController = FindFirstObjectByType<UIController>();
        _levelManager = FindFirstObjectByType<LevelManager>();
        _cameraController.Initialize();
        _pointerInput = new PointerInput(_cameraController.MainCamera);
        _pointerVisualizer = FindFirstObjectByType<PointerInputVisualizer>();
        _pointerVisualizer.Initialize(_pointerInput);
        _uiController.Initialize();
        _levelManager.Initialize(_cameraController, _pointerInput);
        
        // TEMP
        _player = FindFirstObjectByType<PlayerBehaviour>();
        _player.Initialize(_cameraController, _pointerInput, null, null);
        _enemies = FindObjectsByType<EnemyBehaviour>(FindObjectsSortMode.None);
        foreach (var e in _enemies)
            e?.Initialize(_player, null, null);

        if (SoundManager.Instance != null) _gameStateController.OnStateChanged += SoundManager.Instance.OnStateChanged;
        _gameStateController.OnStateChanged += _cameraController.OnStateChanged;
        _gameStateController.OnStateChanged += _uiController.OnStateChanged;
        _gameStateController.OnStateChanged += OnStateChanged;

        _gameStateController.ChangeState(InGameState.Play);
    }

    // ========== 상태 전환 ==========

    private void OnStateChanged(InGameState prev, InGameState curr)
    {

    }

    private void Failure()
    {
        _gameStateController.ChangeState(InGameState.Failure);
    }
    
    private void Success()
    {
        _gameStateController.ChangeState(InGameState.Success);
    }
}
