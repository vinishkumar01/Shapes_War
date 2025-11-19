using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracer_Idle_State : EnemyState
{


    public Tracer_Idle_State(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
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

        ((Tracer)enemy).MoveEnemy(Vector2.zero);

        // Make sure that the NPC always looks at the player until it retreats.
        ((Tracer)enemy).FacingPlayer();

        //Intimidate the Player
        if(!((Tracer)enemy).isIntimidating)
        {
            enemy.StartCoroutine(((Tracer)enemy).IntimidatePlayer());
        }

        //Check if player enters the detection radius
        ((Tracer)enemy).playerDetection();

        if(((Tracer)enemy).isPlayerNear)
        {
            enemy.stateMachine.ChangeState(enemy.attackState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

}
