using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracer_Attack_State : EnemyState
{
    public Tracer_Attack_State(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
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

        enemy.MoveEnemy(Vector2.zero);

        // Make sure that the NPC always looks at the player until it retreats.
        ((Tracer)enemy).FacingPlayer();

        //Check if player enters the detection radius
        ((Tracer)enemy).playerDetection();

        //Attack the Player
        if (((Tracer)enemy).isPlayerNear)
        {
            if(!((Tracer)enemy).isAttacking)
            {
                enemy.StartCoroutine(((Tracer)enemy).AttackPlayer());
            }
            
        }

        //if player is detected Retreat (In Enemy the Chase state is assigned as retreat state) 
        if (((Tracer)enemy).isPlayerDetected)
        {
            enemy.stateMachine.ChangeState(enemy.chaseState);
        }

        // Attack State -> Idle State 
        if (!((Tracer)enemy).isPlayerDetected && !((Tracer)enemy).isPlayerNear)
        {
            enemy.stateMachine.ChangeState(enemy.IdleState);
        }

    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

}
