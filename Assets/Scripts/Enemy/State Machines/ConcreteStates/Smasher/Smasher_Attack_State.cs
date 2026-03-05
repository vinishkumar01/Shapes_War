using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smasher_Attack_State : EnemyState
{
    public Smasher_Attack_State(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
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

        // Cast Rays and Ray Spheres for environment detection
        ((Smasher)enemy).DrawRaysAndSpheres();

        //Flip the NPC when it hits the edge
        ((Smasher)enemy).FlipToAvoidEdges();

        //Move the Character
        ((Smasher)enemy).MoveAndAttack();

        //Switch to idle if player is not active.
        if (!enemy.IsPlayerActive())
        {
            //Debug.Log("Switching to Chase State");
            enemy.stateMachine.ChangeState(enemy.IdleState);
        }

        //Switch to chase state when the player gets near the NPC
        if (!((Smasher)enemy).isPlayerNearToPorformSlam || !((Smasher)enemy).isPlayerNearToPerformJumpAttack)
        {
            enemy.stateMachine.ChangeState(enemy.chaseState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

}
