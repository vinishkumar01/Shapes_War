using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState 
{
    protected Player _player;
    protected PlayerDataSO _playerDataSO;
    protected PlayerStateMachine _playerStateMachine;

    public PlayerState (Player player, PlayerStateMachine playerStateMachine, PlayerDataSO playerDataSO)
    {
        this._player = player;
        this._playerStateMachine = playerStateMachine;
        this._playerDataSO = playerDataSO;
    }

    public virtual void EnterState() { }

    public virtual void ExitState() { }

    public virtual void FrameUpdate() { }

    public virtual void PhysicsUpdate() { }

    public virtual void LateFrameUpdate() { }

    public virtual void AnimationTriggerEvent(Player.AnimationTriggerType triggerType) { }
}
