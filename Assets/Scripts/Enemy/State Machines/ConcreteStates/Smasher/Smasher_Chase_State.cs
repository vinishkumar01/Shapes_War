using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smasher_Chase_State : EnemyState
{
    public Smasher_Chase_State(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
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

        //Move the Character
        ((Smasher)enemy).Move();

        //Flip the NPC when it hits the edge
        ((Smasher)enemy).FlipToAvoidEdges();

        //Flip the NPC when it hits the player
        ((Smasher)enemy).FlipWhenPlayerDetected();

        //Switch to attack state when the player gets near the NPC
        if (((Smasher)enemy).isPlayerNearToPorformSlam || ((Smasher)enemy).isPlayerNearToPerformJumpAttack)
        {
            enemy.stateMachine.ChangeState(enemy.attackState);
        }

        //Switch to idle if player is not active.
        if (!enemy.IsPlayerActive())
        {
            Debug.Log("Switching to Chase State");
            enemy.stateMachine.ChangeState(enemy.IdleState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

}
