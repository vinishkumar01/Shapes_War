using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Security.Cryptography;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class homingMissile : MonoBehaviour, IHittable
{
    [Header("References")]
    [SerializeField] Transform player;
    Rigidbody2D rb;
    [SerializeField] LayerMask playerLayer;
    private FlashEffect _flashEffect;

    [Header("Missile Configs")]
    [SerializeField] int MissileSpeed = 10;
    [SerializeField] float rotateSpeed = 400f;
    [SerializeField] bool isPlayerDetected;
    [SerializeField] bool PlayerCollided;
    [SerializeField] float playerDetectionCheckRadius = 8f;
    [SerializeField] int MissileHealth = 20;

    [Header("Explode Configs")]
    [SerializeField] float impactField;
    [SerializeField] float force;
    [SerializeField] LayerMask explosionHitlayer;

    [Header("Tags")]
    [SerializeField] string[] destTag = { "Player", "Missile"};

    [Header("Explosion Effect")]
    [SerializeField] private GameObject explosionPrefab;

    private void OnEnable()
    {
        MissileHealth = 20;
        if(_flashEffect != null)
        {
            _flashEffect.ResetFlash();
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); 
        _flashEffect = GetComponent<FlashEffect>();

        if(player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        FollowPlayer();
        MissileSpeedAndRotateConfig();
    }

    void IHittable.RecieveHit(RaycastHit2D RayHit)
    {
        Debug.Log("Got Hit: by missile");
        MissileHealth -= 10;

        _flashEffect.CallDamageFlash();

        if (MissileHealth == 0)
        {
            ExplosionEffectAnimation();

            PoolManager.ReturnObjectToPool(gameObject, PoolManager.PoolType.GameObjects);
        }
    }

    void FollowPlayer()
    {
        Vector2 direction = ((Vector2)player.position - rb.position).normalized;

        float rotateAmount = Vector3.Cross(direction, transform.up).z;

        rb.angularVelocity = -rotateAmount * rotateSpeed;

        rb.velocity = transform.up * MissileSpeed;
    }

    private void Update()
    {
        DrawCircleAroundMissile();
    }

    void DrawCircleAroundMissile()
    {
        isPlayerDetected = Physics2D.OverlapCircle(transform.position, playerDetectionCheckRadius, playerLayer);
    }


    void MissileSpeedAndRotateConfig()
    {
        if(isPlayerDetected)
        {
            MissileSpeed = 20;
            rotateSpeed = 800;
        }
        else
        {
            MissileSpeed = 10;
            rotateSpeed = 400;
        }
    }

    void explodeOnContact()
    {
        Collider2D[] objects = Physics2D.OverlapCircleAll(transform.position, impactField, explosionHitlayer);

        foreach(Collider2D obj in objects)
        {
            Vector2 direction = obj.transform.position - transform.position;

            obj.GetComponent<Rigidbody2D>().AddForce(direction * force);
        }
    }


    private void OnDrawGizmos()
    {
        if (isPlayerDetected)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, playerDetectionCheckRadius);
        }
        else
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, playerDetectionCheckRadius);
        }

        if(PlayerCollided)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, impactField);
        }
        else
        {
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(transform.position, impactField);
            }
        }
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach(string t in destTag)
        {
            if (collision.collider.CompareTag(t))
            {
                PlayerCollided = true;
                explodeOnContact();
                ExplosionEffectAnimation();
                PoolManager.ReturnObjectToPool(gameObject, PoolManager.PoolType.GameObjects);
            }
        }

        if (collision.gameObject.TryGetComponent(out IDamageable damageable))
        {
            Vector2 hitDirection = (collision.transform.position - transform.position).normalized;
            damageable.Damage(10f, hitDirection);
        }
    }

    private void ExplosionEffectAnimation()
    {
        PoolManager.SpawnObject(explosionPrefab, transform.position, Quaternion.identity, PoolManager.PoolType.GameObjects);

    }
}
