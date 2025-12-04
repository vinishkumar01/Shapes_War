using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashState : PlayerState
{

    [Header("Player Dash")]
    private bool _isDashing;
    private float _dashTimeLeft;
    private float _lastDash = -100f;

    public PlayerDashState(Player player, PlayerStateMachine playerStateMachine, PlayerDataSO playerDataSO) : base(player, playerStateMachine, playerDataSO)
    {
    }

    public override void AnimationTriggerEvent(Player.AnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
        //_player.AnimationTriggerEvent(Player.AnimationTriggerType.Dash);
    }

    public override void EnterState()
    {
        base.EnterState();

        if (Time.time < (_lastDash + _playerDataSO.dashCoolDown))
        {
            //if the cooldown isnt finished -> go back to grounded/ air state

            //if/else if/ if ternary operator
            //first it checks if the player is grounded if no -> then changes to player jump state, if yes -> then it comes to check the middle condition check if button pressed for hmovement if yes -> state changes to Move else changes to idle state
            _playerStateMachine.ChangeState(_player._isGrounded ? (Mathf.Abs(_player.MovementInputXDirection) > 0.01f ? _player._playerMoveState : _player._playerIdleState) : _player._playerJumpState);
            return;
        }

        _isDashing = true;
        _dashTimeLeft = _playerDataSO.dashTime;
        _lastDash = Time.time;

        _player._animator.SetBool("isDashing", true);
    }

    public override void ExitState()
    {
        base.ExitState();

        _isDashing = false;
        _player._dashText.text = " ";

        _player._animator.SetBool("isDashing", false);
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        if (!_isDashing)
            return;

        _dashTimeLeft -= Time.deltaTime;

        if (!_player._knockBack.IsBeingKnockedBack)
        {
            Dash();
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

    private void Dash()
    {

        if (!_isDashing || _player._knockBack.IsBeingKnockedBack)
            return;

        float XMove = _player.MovementInputXDirection;

        //If the player is not moving and pressed the Dash button we will make the player to dash on the direction its facing
        if (Mathf.Abs(XMove) < 0.01f)
            XMove = _player.transform.localScale.x > 0 ? 1f : -1f;

        _player.RB.velocity = new Vector2(XMove * _playerDataSO.dashSpeed, 0);

        if (Time.time <= (_lastDash + _playerDataSO.dashCoolDown))
        {
            _player._dashText.text = "Dash Recharging";
        }
        else
        {
            _player._dashText.text = " ";
        }
    }

    private void StateTransitions()
    {
        if (_dashTimeLeft <= 0f)
        {
            //Decide where to transist after dash
            if (!_player._isGrounded && _player.RB.velocity.y != 0f)
            {
                _playerStateMachine.ChangeState(_player._playerJumpState);
            }

            else if (Mathf.Abs(_player.MovementInputXDirection) > 0.01f)
            {
                _playerStateMachine.ChangeState(_player._playerMoveState);
            }
            else
            {
                _playerStateMachine.ChangeState(_player._playerIdleState);
            }
        }

    }
}
