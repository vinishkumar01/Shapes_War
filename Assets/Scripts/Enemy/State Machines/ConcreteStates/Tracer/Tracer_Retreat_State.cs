using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Tracer_Retreat_State : EnemyState
{


    public Tracer_Retreat_State(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
    }

    public override void AnimationTriggerEvent(Enemy.AnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
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

        //Check if player enters the detection radius
        ((Tracer)enemy).playerDetection();

        //Check Surrounding:
        ((Tracer)enemy).SurroundingCheck();

        //Attack the Player
        if (((Tracer)enemy).isPlayerNear)
        {
            //Debug.Log("Player is Near");
            if (!((Tracer)enemy).isAttacking && enemy.IsPlayerActive())
            {
                enemy.StartCoroutine(((Tracer)enemy).AttackPlayer());
            }

        }

        if(((Tracer)enemy).path.Count == 0)
        {
            //Retreat State -> Attack State 
            if (!((Tracer)enemy).isPlayerDetected && ((Tracer)enemy).isPlayerNear)
            {
                //Debug.Log("Player is Near and Player is Not Detected");
                enemy.stateMachine.ChangeState(enemy.attackState);
            }

            //Retreat State -> Idle State
            if (!((Tracer)enemy).isPlayerDetected && !((Tracer)enemy).isPlayerNear)
            {
                //Debug.Log("Player is not Near and Player is Not Detected");
                enemy.stateMachine.ChangeState(enemy.IdleState);
            }
        }
        
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (GameState.CanPlayerControl)
        {
            ((Tracer)enemy).Retreat();
        }
        else
        {
            //Stopping the sound manually
            enemy._audioSource.Stop();
        }
    }

}
