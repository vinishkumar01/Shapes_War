using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] string[] bulletInteractable = new string[] { "Enemy", "Platform" };
    [SerializeField] LayerMask bulletInteractables;

    [Header("Bullet Configs")]
    [SerializeField] float bulletSpeed = 150f;
    [SerializeField] bool Collided;
    [SerializeField] Vector3 SpawnPosition;
    [SerializeField] Vector3 lastPosition;
    [SerializeField] float BulletMaxTravelDistance = 30f;

    [Header("Particle Effect")]
    [SerializeField] GameObject Bullet_Collision;

    private void OnEnable()
    {
        Collided = false;
        rb.simulated = true;
        bulletResetConfigs();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        lastPosition = transform.position;
    }

    private void OnDisable()
    {
        if(Collided)
            StopAllCoroutines();
    }

    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    //private void Update()
    //{
    //    // if the bullet is not collided with any thing and is travelled nearly the max distance the bullet is returned to pool
    //    if(!Collided && Vector3.Distance(SpawnPosition, transform.position) > BulletMaxTravelDistance)
    //    {
    //        rb.simulated = false;
    //        PoolManager.ReturnObjectToPool(gameObject, PoolManager.PoolType.GameObjects);
    //    }
    //}

    private void FixedUpdate()
    {
        if (Collided) return;

        Vector3 currentPosition = transform.position;
        Vector3 direction = currentPosition - lastPosition;
        float distance = direction.magnitude;

        if(distance > 0f)
        {
            RaycastHit2D hit = Physics2D.Raycast(lastPosition, direction.normalized, distance);

            if(hit.collider != null)
            {
                Debug.Log($"Bullet hit: {hit.collider.name} | Tag: {hit.collider.tag}");

                if (hit.collider.CompareTag("Platform") || hit.collider.CompareTag("Enemy"))
                {
                    HandleCollision(hit.collider, hit.point);
                }
            }
            else if(hit.collider == null && Vector3.Distance(SpawnPosition, transform.position) > BulletMaxTravelDistance)
            {
                rb.simulated = false;
                PoolManager.ReturnObjectToPool(gameObject, PoolManager.PoolType.GameObjects);
            }
        }

        lastPosition = currentPosition;
    }

    public void bulletForce( Vector3 direction)
    {
        SpawnPosition = transform.position;
        rb.velocity = direction * bulletSpeed;
    }

    void bulletResetConfigs()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;
        rb.WakeUp();
    }

    private void HandleCollision(Collider2D collider, Vector2 hitPoint)
    {
        if (Collided) return;
        Collided = true;

        //Particle
        PoolManager.SpawnObject(Bullet_Collision, hitPoint, Quaternion.identity, PoolManager.PoolType.ParticleSystem);

        //Detach trail before Pooling 
        var trail = GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.transform.parent = null;
            // just taking the reference to avoid returning null if bullet is reused
            var detachedTrail = trail;
            StartCoroutine(ReturnTrailAfterTime(detachedTrail, detachedTrail.time));
            //effectsManager.RunCoroutine(ReturnTrailAfterTime(detachedTrail, detachedTrail.time));
        }

        //reset before returning
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = false;
        rb.Sleep();


        StartCoroutine(returnBulletinNextFrame());
    }

    IEnumerator ReturnTrailAfterTime(TrailRenderer trail, float time)
    {
        yield return new WaitForSeconds(time);

        if(trail != null && trail.gameObject.activeInHierarchy)
        {
            trail.Clear();
            PoolManager.ReturnObjectToPool(trail.gameObject, PoolManager.PoolType.GameObjects);
        }
        
    }

    IEnumerator returnBulletinNextFrame()
    {
        yield return null;
        PoolManager.ReturnObjectToPool(gameObject, PoolManager.PoolType.GameObjects);
    }
}
