using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smasher_Idle_State : EnemyState
{
    public Smasher_Idle_State(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
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

        enemy.MoveEnemy(Vector2.zero);

        if (enemy.IsPlayerActive())
        {
            //Debug.Log("Switching to Chase State");
            enemy.stateMachine.ChangeState(enemy.chaseState);
        }

    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

}
