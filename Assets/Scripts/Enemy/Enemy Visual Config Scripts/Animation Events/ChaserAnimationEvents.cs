using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaserAnimationEvents : MonoBehaviour
{
    private Enemy _enemy;

    private void Awake()
    {
        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        _enemy = enemy?.GetComponent<Enemy>();
    }

    private void OnEnemyIdle()
    {
        _enemy.AnimationTriggerEvent(Enemy.AnimationTriggerType.EnemyIdle);
    }

    private void OnEnemyReadyToRun()
    {
        _enemy.AnimationTriggerEvent(Enemy.AnimationTriggerType.ReadyToRun);
    }

    private void OnEnemyRun()
    {
        _enemy.AnimationTriggerEvent(Enemy.AnimationTriggerType.Run);
    }

    private void OnEnemyJump()
    {
        _enemy.AnimationTriggerEvent(Enemy.AnimationTriggerType.Jump);
    }

    private void OnEnemyInAir()
    {
        _enemy.AnimationTriggerEvent(Enemy.AnimationTriggerType.InAir);
    }

    private void OnEnemyFall()
    {
        _enemy.AnimationTriggerEvent(Enemy.AnimationTriggerType.Fall);
    }

    private void OnEnemyLand()
    {
        _enemy.AnimationTriggerEvent(Enemy.AnimationTriggerType.Land);
    }

    private void OnEnemyAttack()
    {
        _enemy.AnimationTriggerEvent(Enemy.AnimationTriggerType.Attack);
    }

    private void OnEnemyDamaged()
    {
        _enemy.AnimationTriggerEvent(Enemy.AnimationTriggerType.EnemyDamaged);
    }
}
