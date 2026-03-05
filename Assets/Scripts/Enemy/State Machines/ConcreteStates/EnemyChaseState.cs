using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class EnemyChaseState : EnemyState
{
    private Transform _playerTransform;

    private float _MovementSpeed = 5f;

    // As we are instantiating this class in Enemy - Awake function this constructor too act like an awake function
    public EnemyChaseState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

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

        Vector2 moveDirection = (_playerTransform.position - enemy.transform.position).normalized;

        enemy.MoveEnemy(moveDirection * _MovementSpeed);

        //if (enemy.IsWithinStrikingDistance)
        //{
        //    enemy.stateMachine.ChangeState(enemy.attackState);
        //}
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}
