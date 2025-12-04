using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerState
{
    [Header("Movement Configs")]
    private float VelocityXSmoothing;

    public PlayerMoveState(Player player, PlayerStateMachine playerStateMachine, PlayerDataSO playerDataSO) : base(player, playerStateMachine, playerDataSO)
    {
    }

    public override void AnimationTriggerEvent(Player.AnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
        //_player.AnimationTriggerEvent(Player.AnimationTriggerType.Run);
    }

    public override void EnterState()
    {
        base.EnterState();
        _player._animator.SetBool("isMoving", true);
    }

    public override void ExitState()
    {
        base.ExitState();
        _player._animator.SetBool("isMoving", false);
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        if (!_player._knockBack.IsBeingKnockedBack)
        {
            MovePlayer();
        }

        StateTransitions();
    }

    public override void LateFrameUpdate()
    {
        base.LateFrameUpdate();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    private void MovePlayer()
    {
        float TargetVelocityX = _playerDataSO.movementSpeed * _player.MovementInputXDirection;

        _player.RB.velocity = new Vector2(Mathf.SmoothDamp(_player.RB.velocity.x, TargetVelocityX, ref VelocityXSmoothing, (_player._isGrounded) ? _playerDataSO.accelerationTimeGround : _playerDataSO.accelerationTimeAirBorne), _player.RB.velocity.y);

        //Check if player is Attached to Rope, if attached then we can make to swing while hanging
        if (_player._attachedToRope)
        {
            _player.RB.AddRelativeForce(new Vector2(_player.MovementInputXDirection * _playerDataSO.ropeSwingForce, _player.RB.velocity.y));
        }
    }

    private void StateTransitions()
    {
        //Transit to idle
        if (Mathf.Abs(_player.MovementInputXDirection) <= 0.01f)
        {
            _playerStateMachine.ChangeState(_player._playerIdleState);
        }

        //Transit to Jump
        if (_player.JumpPressed)
        {
            _playerStateMachine.ChangeState(_player._playerJumpState);
        }

        //Transist to Dash
        if (_player.DashPressed)
        {
            _playerStateMachine.ChangeState(_player._playerDashState);
        }
    }
}
