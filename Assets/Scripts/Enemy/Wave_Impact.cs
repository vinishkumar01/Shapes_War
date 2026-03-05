using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Wave_Impact : MonoBehaviour
{
    private int _smasherWavesImpactGives { get; set; }

    private void OnEnable()
    {
        GameObject smasherPrefab = GameManager._instance.GetPrefabByEnemyType(EnemyType.Smasher);

        if(GameManager._instance != null && GameManager._instance.TryGetEnemyData(smasherPrefab, out var data))
        {
            _smasherWavesImpactGives = data._smasherWaveAttack;
        }
    }

    [Header("Wave Impact configs")]
    [SerializeField] float force;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.TryGetComponent(out IPlayerDamageable damageable))
        {
            Vector2 hitDirection = (collision.transform.position - transform.position).normalized;

            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(hitDirection * force);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, hitDirection, 1f, LayerMask.GetMask("Player"));

            
            damageable.Damage(_smasherWavesImpactGives, hitDirection, hit.point, hit.normal);
        }
    }
}
