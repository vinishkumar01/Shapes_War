using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Security.Cryptography;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class homingMissile : MonoBehaviour, IDamageable, IUpdateObserver, IFixedUpdateObserver
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    private Rigidbody2D rb;
    [SerializeField] private LayerMask playerLayer;
    private FlashEffect _flashEffect;
    private GameObject _ownerTracer;
    private AudioSource _audioSource;

    private Transform _cameraTransform;

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

    [Header("SFX")]
    [SerializeField] private AudioClip _missileExplosionEffect;

    public int MaxHealth { get => MissileMaxHealth; set => MissileMaxHealth = value; }
    public int CurrentHealth { get => MissileCurrentHealth; set => MissileCurrentHealth = value; }
    public int DamageAmount { get => MDamageAmount; set => MDamageAmount = value; }

    private void OnEnable()
    {
        //Register to Update Manager
        UpdateManager.RegisterObserver(this);
        //Register to FixedUpdate Manager
        FixedUpdateManager.RegisterObserver(this);

        //Registering this Missile in the Indicator manager
        TargetIndicaotrManager._instance.OnTracerMissileSpawned(_ownerTracer, this.gameObject);

        hasReturnedToPool = false;
        if(_flashEffect != null)
        {
            _flashEffect.ResetFlash();
        }
    }

    public void SetOwnerTracer(GameObject tracer)
    {
        //getting the tracer which shot this missile
        _ownerTracer = tracer;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        _flashEffect = GetComponent<FlashEffect>();
        _audioSource = GetComponent<AudioSource>();

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
    public void ObservedFixedUpdate()
    {
        playerNotFound();
        MissileSpeedAndRotateConfig();

        if(playerTransform != null)
        {
            FollowPlayer();

            //Applying missile travel sound effect
            if(!_audioSource.isPlaying)
            {
                _audioSource.Play();
            }
            
        }
    }

    void IDamageable.RecieveHit(RaycastHit2D RayHit, Vector2 hitDirection)
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

        if(_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }

    #region Spatial Effect

    bool isInsideCameraView()
    {
        Vector3 viewPortPos = Camera.main.WorldToViewportPoint(transform.position);

        return viewPortPos.x >= 0 && viewPortPos.x <= 1 && viewPortPos.y >= 0 && viewPortPos.y <= 1;
    }

    private void UpdateMissileAudioSource()
    {
        if (_cameraTransform == null)
        {
            //getting the camera transform
            _cameraTransform = Camera.main.transform;
        }

        if (_audioSource == null || !_audioSource.isPlaying)
        { return; }

        float dx = transform.position.x - _cameraTransform.position.x;
        float distance = Mathf.Abs(dx);

        //Volume based on distance
        float maxDistance = 10f;

        float volume = 1f - Mathf.Clamp01(distance / maxDistance);

        if (isInsideCameraView())
        {
            volume = Mathf.Max(volume, 0.4f);
            _audioSource.panStereo = 0f;
        }


        volume = Mathf.SmoothStep(0f, 1f, volume);
        _audioSource.volume = volume;

        // left right panning
        float pan = Mathf.Clamp(dx / maxDistance, -1f, 1f);
        _audioSource.panStereo = pan;
    }

    #endregion

    void FollowPlayer()
    {
        Vector2 direction = ((Vector2)playerTransform.position - rb.position).normalized;

        float rotateAmount = Vector3.Cross(direction, transform.up).z;

        rb.angularVelocity = -rotateAmount * rotateSpeed;

        rb.velocity = transform.up * MissileSpeed;
    }

    public void ObservedUpdate()
    {
        DrawCircleAroundMissile();
        UpdateMissileAudioSource();

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

        SFXManager._instance.playSFX(_missileExplosionEffect, transform.position, 1f, true, false);
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

                //Delete this specific from the Missile list 
                GameManager._instance._missilesList.Remove(this.gameObject);
            }
        }

        if (collision.gameObject.TryGetComponent(out IPlayerDamageable damageable))
        {
            ContactPoint2D contact = collision.GetContact(0);
            Vector2 hitPoint = contact.point;
            Vector2 hitNormal = contact.normal;

            Vector2 hitDirection = (collision.transform.position - transform.position).normalized;
            damageable.Damage(10, hitDirection, hitPoint, hitNormal);
        }
    }

    private void OnDisable()
    {
        //Register to Update Manager
        UpdateManager.UnregisterObserver(this);
        //Register to FixedUpdate Manager
        FixedUpdateManager.UnregisterObserver(this);

        //Registering this Missile in the Indicator manager
        TargetIndicaotrManager._instance.OnTracerMissileDestroyed(_ownerTracer, this.gameObject);
    }
}
