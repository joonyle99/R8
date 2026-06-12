using System;
using UnityEngine;
using JoonyleGameDevKit;

public abstract class EnemyBehaviour : CombatEntity
{
    private PlayerBehaviour _player;
    public PlayerBehaviour Player => _player;
    
    public void Initialize(PlayerBehaviour player, Action<int> onDamaged, Action onDead)
    {
        InitCombatEntity(onDamaged, onDead);

        _player = player;

        OnInitialize();
    }

    // ============ ... ============

    protected abstract void OnInitialize();
}
