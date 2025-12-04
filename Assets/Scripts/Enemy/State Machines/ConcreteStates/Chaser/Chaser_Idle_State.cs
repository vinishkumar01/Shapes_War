using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class Chaser_Idle_State : EnemyState
{

    // As we are instantiating this class in Enemy - Awake function this constructor too act like an awake function
    public Chaser_Idle_State(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {         

    }

    public override void AnimationTriggerEvent(Enemy.AnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
        
        if(triggerType == Enemy.AnimationTriggerType.EnemyIdle)
        {
            //Debug.Log("Idle animation event recieved by state machine");
        }
    }

    public override void EnterState()
    {
        base.EnterState();
        
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        //Debug.Log("Idle State FrameUpdate running");

        ((Chaser)enemy).MoveEnemy(Vector2.zero);

        if(enemy.IsPlayerActive())
        {
            //Debug.Log("Switching to Chase State");
            enemy.stateMachine.ChangeState(enemy.chaseState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void LateFrameUpdate()
    {
        base.LateFrameUpdate();
        ((Chaser)enemy).FreezeSprite();
    }
}
