using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerState
{
    [Header("Movement Configs")]
    private float VelocityXSmoothing;

    [Header("Player Movement Squash Config")]
    private float _prevVelocity;
    private Vector2 _applyMoveSquash;
    private bool _wasMoving;
    //Landing Squash
    private bool _wasGrounded;
    private Vector2 _applyLandingSquash;

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

        if (_player._isGrounded)
        {
            //SFXManager._instance.PlayMovementSoundFXClip(_player._slideSoundClip, _player.transform, 1f, true);
            _player._audioSource.clip = _player._slideSoundClip;
            _player._audioSource.Play();
        }
    }

    public override void ExitState()
    {
        base.ExitState();
        _player._animator.SetBool("isMoving", false);

        _player._audioSource.Stop();

        //_player._dust.Stop();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        //Constantly check if the player is touching the ground to control the movement SFX
        if (!_player._isGrounded)
        {
            _player._audioSource.Stop();
        }

        //Dust Effect
        _player._dust.Play();

        MovePlayer();

        DetectMoveAndLandingSquash();

        StateTransitions();
    }

    public override void LateFrameUpdate()
    {
        ApplyMoveSquashAndStretch();
        ApplyLandingSquashAndStretch();

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

            //If the player is Attached to the Rope and pressed jump
            if (_player.JumpPressed)
            {
                _player.Detach();
                //also we are transitioning to the jump state
                _playerStateMachine.ChangeState(_player._playerJumpState);
            }
        }
    }

    private void DetectMoveAndLandingSquash()
    {
        float moveStartThreshold = 0.05f;
        float vx = _player.RB.velocity.x;

        bool isMoving  = Mathf.Abs(_prevVelocity) > moveStartThreshold;

        //Applying squash when the player starts moving
        if(isMoving && !_wasMoving)
        {
            _applyMoveSquash += new Vector2(0.09f, -0.04f);
        }

        //Applying and squash and stretch when changing direction
        if(Mathf.Sign(_prevVelocity) != Mathf.Sign(vx) && isMoving)
        {
            _applyMoveSquash += new Vector2(0.1f, -0.05f);
        }

        //Checking for Landing
        if(_player._isGrounded && !_wasGrounded)
        {
            float impactSpeed = Mathf.Abs(_player.RB.velocity.y);
            float t = Mathf.InverseLerp(3f, 12f, impactSpeed);

            float x = Mathf.Lerp(0.10f, 0.25f, t);
            float y = Mathf.Lerp(-0.15f, -0.35f, t);

            _applyLandingSquash += new Vector2(x, y);
        }


        _wasMoving = isMoving;
        _prevVelocity = vx;

        //Landing
        _wasGrounded = _player._isGrounded;
    }

    private void ApplyMoveSquashAndStretch()
    {
        if (_applyMoveSquash != Vector2.zero)
        {
            _player._playerSquashandStretch.Squash(_applyMoveSquash.x, _applyMoveSquash.y);

            _applyMoveSquash = Vector2.zero;
        }
    }

    private void ApplyLandingSquashAndStretch()
    {
        if (_applyLandingSquash != Vector2.zero)
        {
            _player._playerSquashandStretch.Squash(_applyLandingSquash.x, _applyLandingSquash.y);

            _applyLandingSquash = Vector2.zero;
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
        if (_player.DashPressed && _player._playerDataSO.dashCount > 0)
        {
            _playerStateMachine.ChangeState(_player._playerDashState);
        }
    }
}
