using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class Chaser_Chase_State : EnemyState
{


    // As we are instantiating this class in Enemy - Awake function this constructor too act like an awake function
    public Chaser_Chase_State(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
        
    }

    public override void AnimationTriggerEvent(Enemy.AnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);

        if (triggerType == Enemy.AnimationTriggerType.ReadyToRun)
        {
            //Debug.Log("ReadyToRun to Run animation event recieved by state machine");
        }
    }

    public override void EnterState()
    {
        base.EnterState();
        //Debug.Log("Entered Chase State");
        enemy._animator.SetBool("IsStartedMoving", true);
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        ((Chaser)enemy).groundedAndplatformCheck();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        ((Chaser)enemy).FollowPlayer();

        if (!enemy.IsPlayerActive())
        {
            enemy.stateMachine.ChangeState(enemy.IdleState);
        }
    }

    public override void LateFrameUpdate()
    {
        base.LateFrameUpdate();

        ((Chaser)enemy).FreezeSprite();
    }

}
