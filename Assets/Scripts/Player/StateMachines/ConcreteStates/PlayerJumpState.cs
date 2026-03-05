using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerState
{
    [Header("Move in Air")]
    private float VelocityXSmoothing;


    private bool _jumpWasExecuted = false; // Track if the jump was performed
    private bool _hasDoubleJumped = false;

    private float _groundExitLockTime = 0.1f;

    public PlayerJumpState(Player player, PlayerStateMachine playerStateMachine, PlayerDataSO playerDataSO) : base(player, playerStateMachine, playerDataSO)
    {
    }

    public override void AnimationTriggerEvent(Player.AnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
        //_player.AnimationTriggerEvent(Player.AnimationTriggerType.Jump);
    }

    public override void EnterState()
    {
        base.EnterState();

        _player._animator.SetBool("isJumping", true);
        _jumpWasExecuted = false;

        _groundExitLockTime = 0.1f;
    }

    public override void ExitState()
    {
        base.ExitState();
        _player._animator.SetBool("isJumping", false);
        _jumpWasExecuted = false;
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        _groundExitLockTime -= Time.deltaTime;

        if (!_player._knockBack.IsBeingKnockedBack)
        {
            Jump();
            MovePlayerInAir();
        }

        StateTransitions();
    }

    public override void LateFrameUpdate()
    {
        ApplyingSqaushAndStretch();

        base.LateFrameUpdate();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    private void Jump()
    {
        //Ground / coyote jump
        if (_player._jumpBufferTimeCounter > 0f && _player._coyoteTimeCounter > 0f && !_jumpWasExecuted)
        {
            _player._isJumping = true;
            _jumpWasExecuted = true;

            _player._jumpTimeCounter = _playerDataSO.jumpTime;
            _player.RB.velocity = new Vector2(_player.RB.velocity.x, _player._jumpForce);

            //reset the jump Buffer Counter 
            _player._jumpBufferTimeCounter = 0f;

            //Debug.Log("Ground jump executed");
        }

        //DoubleJump Configs checking if the player is grounded, if yes then we make the doubleJumped flag to false.
        if(_player._isGrounded)
        {
            _hasDoubleJumped = false;
        }

        //Double Jump - We got to trigger only once in mid air
        else if (_player._jumpBufferTimeCounter > 0f && !_player._isGrounded && _player._doubleJump && !_hasDoubleJumped && _playerDataSO.doubleJumpSkill)
        {
            _player._isJumping = true;
            _player._jumpTimeCounter = _playerDataSO.jumpTime;

            _player.RB.velocity = new Vector2(_player.RB.velocity.x, _playerDataSO.doubleJumpForce);

            //Flag hasDoubleJumped
            _hasDoubleJumped = true;

            _playerDataSO.doubleJumpCount--;
            _player._doubleJumpCountUI.text = _playerDataSO.doubleJumpCount.ToString();

            if (_playerDataSO.doubleJumpCount <= 0)
            {
                _playerDataSO.doubleJumpSkill = false;
                _player._doubleJump = false;
                _player._doubleJumpSkill.text = _playerDataSO.doubleJumpSkill.ToString();
            }

            //reset the jump Buffer Counter 
            _player._jumpBufferTimeCounter = 0f;
        }

        //Jump button been held/ Mario jump style (Variable height jump)
        if (_player.JumpHeld && _player._isJumping && _player._jumpTimeCounter > 0f && _player.RB.velocity.y > 0f)
        {
            _player.RB.velocity = new Vector2(_player.RB.velocity.x, _player._jumpForce);
            _player._jumpTimeCounter -= Time.deltaTime;
        }
        else if(_player._isJumping && _player._jumpTimeCounter <= 0f)
        {
            _player._isJumping = false;
        }

        //Jump button was Released
        if (_player.JumpReleased)
        {
            _player._isJumping = false;
        }
    }

    private void ApplyingSqaushAndStretch()
    {
        //Applying Stretch When Jumping
        if (_jumpWasExecuted)
        {
            if (_player.IsFacingRight)
            {
                _player._playerSquashandStretch.Squash(-0.08f, 0.03f);
            }
            else
            {
                _player._playerSquashandStretch.Squash(0.08f, 0.03f);
            }
        }

        //Landing Squash 
        if (!_player._isJumping && _jumpWasExecuted)
        {
            if (_player._isGrounded)
            {
                if(_player.IsFacingRight)
                {
                    _player._playerSquashandStretch.Squash(0.9f, -0.9f);
                }
                else
                {
                    _player._playerSquashandStretch.Squash(-0.9f, -0.9f);
                }
                
            }
        }

    }

    private void MovePlayerInAir()
    {
        float TargetVelocityX = _playerDataSO.airMovementSpeed * _player.MovementInputXDirection;

        _player.RB.velocity = new Vector2(Mathf.SmoothDamp(_player.RB.velocity.x, TargetVelocityX, ref VelocityXSmoothing, _playerDataSO.accelerationTimeAir), _player.RB.velocity.y);

        //Check if player is Attached to Rope, if attached then we can make to swing while hanging
        if (_player._attachedToRope)
        {
            _player.RB.AddRelativeForce(new Vector2(_player.MovementInputXDirection * _playerDataSO.ropeSwingForceInJump, _player.RB.velocity.y));
        }
    }

    private void StateTransitions()
    {
        //Transist to Dash
        if (_player.DashPressed)
        {
            _playerStateMachine.ChangeState(_player._playerDashState);
            return;
        }

        if (_groundExitLockTime <= 0f && _player._isGrounded)
        {
            if (Mathf.Abs(_player.MovementInputXDirection) > 0.01f)
            {
                _playerStateMachine.ChangeState(_player._playerMoveState);
            }
            else if (Mathf.Abs(_player.MovementInputXDirection) <= 0f)
            {
                _playerStateMachine.ChangeState(_player._playerIdleState);
            }
        }

        //This might be a problem but i want to implement any way
        if(_player._attachedToRope)
        {
            _playerStateMachine.ChangeState(_player._playerIdleState);
        }
    }
}
