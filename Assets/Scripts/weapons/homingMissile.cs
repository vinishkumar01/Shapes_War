using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Security.Cryptography;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class homingMissile : MonoBehaviour, IDamageable
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    private Rigidbody2D rb;
    [SerializeField] private LayerMask playerLayer;
    private FlashEffect _flashEffect;

    [Header("Missile Configs")]
    [SerializeField] int MissileSpeed = 10;
    [SerializeField] float rotateSpeed = 400f;
    [SerializeField] bool isPlayerDetected;
    [SerializeField] bool PlayerCollided;
    [SerializeField] bool hasReturnedToPool = false;
    [SerializeField] float playerDetectionCheckRadius = 8f;

    [Header("Missile Health")]
    [SerializeField] int MissileMaxHealth = 20;
    [SerializeField] int MissileCurrentHealth;
    [SerializeField] int MDamageAmount;

    [Header("Explode Configs")]
    [SerializeField] float impactField;
    [SerializeField] float force;
    [SerializeField] LayerMask explosionHitlayer;

    [Header("Tags")]
    [SerializeField] string[] destTag = { "Player", "Missile"};

    [Header("Explosion Effect")]
    [SerializeField] private GameObject explosionPrefab;

    public int MaxHealth { get => MissileMaxHealth; set => MissileMaxHealth = value; }
    public int CurrentHealth { get => MissileCurrentHealth; set => MissileCurrentHealth = value; }
    public int DamageAmount { get => MDamageAmount; set => MDamageAmount = value; }

    private void OnEnable()
    {
        hasReturnedToPool = false;
        if(_flashEffect != null)
        {
            _flashEffect.ResetFlash();
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        _flashEffect = GetComponent<FlashEffect>();

        if(playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            playerTransform = player?.GetComponent<Transform>();
            if (player == null)
            {
                Debug.LogWarning("Player not found");
            }
        }
        
        
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        playerNotFound();
        FollowPlayer();
        MissileSpeedAndRotateConfig();
    }

    void IDamageable.RecieveHit(RaycastHit2D RayHit)
    {
        Debug.Log("Got Hit: by missile");

        MissileCurrentHealth -= MDamageAmount;

        _flashEffect.CallDamageFlash();

        if (MissileCurrentHealth < 1)
        { 
            ExplosionEffect();
            Die();
        }
    }

    public void Die()
    {
        ReturnToPoolOnce();
    }

    void FollowPlayer()
    {
        Vector2 direction = ((Vector2)playerTransform.position - rb.position).normalized;

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

    void ExplosionEffect()
    {
        GameObject explosion = PoolManager.SpawnObject(explosionPrefab, transform.position, Quaternion.identity, PoolManager.PoolType.GameObjects);
    }

    void ReturnToPoolOnce()
    {
        if(hasReturnedToPool) return;
        hasReturnedToPool = true;
        PoolManager.ReturnObjectToPool(gameObject, PoolManager.PoolType.GameObjects);
    }

    private void playerNotFound()
    {
        if (playerTransform == null)
        {
            Invoke("ReturnToPoolOnce", 2f);
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

                //Return the Missile to the Pool
                ReturnToPoolOnce();

                //Play Explosion Effect
                ExplosionEffect();


            }
        }

        if (collision.gameObject.TryGetComponent(out IPlayerDamageable damageable))
        {
            Vector2 hitDirection = (collision.transform.position - transform.position).normalized;
            damageable.Damage(10, hitDirection);
        }
    }
}
