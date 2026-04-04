using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashState : PlayerState
{

    [Header("Player Dash")]
    private float _dashTimeLeft;
    public float _lastDash = -100f;
    private float _defaultGravity;

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

        _player._isDashing = true;
        _dashTimeLeft = _playerDataSO.dashTime;
        _lastDash = Time.time;

        //setting the current frame x coordinates of the player transform to the lastImageXPos
        _player._lastImageXpos = _player.transform.position.x;

        _defaultGravity = _player.RB.gravityScale;
        

        _player._animator.SetBool("isDashing", true);

        if(_playerDataSO.dashSkill)
        {
            //Setting the gravity to 0 so that when dashing to stay in air
            _player.RB.gravityScale = 0;
            _player.RB.velocity = new Vector2(_player.RB.velocity.x, 0f);

            Dash();
            SFXManager._instance.playSFX(_player._dashSoundClip, _player.transform.position, 1f, false, false);
        }

        _player._dust.Play();
    }

    public override void ExitState()
    {
        base.ExitState();

        _player._isDashing = false;
        _player.RB.gravityScale = _defaultGravity;
        _player._dashText.text = " ";

        _player._animator.SetBool("isDashing", false);

        _player._dust.Play();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        if (!_player._isDashing)
            return;

        _dashTimeLeft -= Time.deltaTime;

        if (_playerDataSO.dashSkill)
        {
            var v = _player.RB.velocity;
            _player.RB.velocity = new Vector2(v.x, 0f);
        }

        //Instantiating After Images to give the dash Effects
        if (Mathf.Abs(_player.transform.position.x - _player._lastImageXpos) > _player._distancebetweenImages)
        {
            PoolManager.SpawnObject(_player._playerAfterImage, _player.transform.position, Quaternion.identity, PoolManager.PoolType.PlayerAfterimage);
            _player._lastImageXpos = _player.transform.position.x;
        }

        StateTransitions();
    }

    public override void LateFrameUpdate()
    {
        //Applying Squash and stretch while dashing
        if (_player.IsFacingRight)
        {
            _player._playerSquashandStretch.Squash(-0.12f, 0.08f);
        }
        else
        {
            _player._playerSquashandStretch.Squash(0.12f, 0.08f);
        }

        base.LateFrameUpdate();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();


    }

    private void Dash()
    {
        //Damage Flash
        _player._flashEffect.CallDashFlash();

        if (_player._knockBack.IsBeingKnockedBack)
            return;

        float XMove = _player.MovementInputXDirection;

        //If the player is not moving and pressed the Dash button we will make the player to dash on the direction its facing
        if (Mathf.Abs(XMove) < 0.01f)
        {
            if(_player.IsFacingRight)
            {
                XMove = 1f;
            }
            else if(!_player.IsFacingRight)
            {
                XMove = -1f;
            }
        }

        _player.RB.velocity = new Vector2(XMove * _playerDataSO.dashSpeed, 0);

        #region Dash UI
        _playerDataSO.dashCount--;
        

        if(_playerDataSO.dashCount <= 0)
        {
            _playerDataSO.dashSkill = false;
        }

        UIManager.InvokeDashCoolDownUpdate(0f,_playerDataSO.dashCoolDown, _playerDataSO.dashCount);
        _player._dashText.text = "Dash Recharging";
        #endregion
    }

    private void StateTransitions()
    {
        if (_dashTimeLeft <= 0f)
        {
            //Decide where to transist after dash
            if (!_player._isGrounded)
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
