using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.TryGetComponent(out IPlayerDamageable damageable))
        {
            GameManager._instance._playerGotInDeathZone = true;
           damageable.Die();

        }

        if(collision.gameObject.CompareTag("Enemy"))
        {
            GameManager._instance.EnemyDestroyed(collision.gameObject);

            //And Pool the Enemy
            PoolManager.ReturnObjectToPool(collision.gameObject, PoolManager.PoolType.GameObjects);
        }
    }
}
