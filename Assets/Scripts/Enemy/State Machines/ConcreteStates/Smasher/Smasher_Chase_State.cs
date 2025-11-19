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

        enemy.MoveEnemy(Vector2.zero);

    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

}
