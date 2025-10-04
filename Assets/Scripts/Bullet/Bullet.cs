using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] string[] bulletInteractable = new string[] { "Enemy", "Platform" };

    [Header("Bullet Configs")]
    [SerializeField] float bulletLife = 1.5f;
    [SerializeField] float bulletSpeed = 150f;
    [SerializeField] bool Collided;

    [Header("Particle Effect")]
    [SerializeField] GameObject Bullet_Collision;

    private void OnEnable()
    {
        Collided = false;
        rb.simulated = true;

        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        StartCoroutine(DisableBulletAfterSeconds(bulletLife));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void bulletForce( Vector3 direction)
    {
        //Vector3 MousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Vector2 Direction = (MousePos - transform.position).normalized;
        //Vector2 rotation = transform.position - MousePos;

        //This makes the bullet to follow the Mouse position every frame;  and call it in Update or FixedUpdate() method
        //rb.velocity = Direction * bulletSpeed;

        bulletResetConfigs();

        rb.velocity = direction * bulletSpeed;
        //float BulletRot = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;
        //transform.rotation = Quaternion.Euler(0, 0, BulletRot);
    }

    void bulletResetConfigs()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;
        rb.WakeUp();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Collided) return;

        foreach (var tag in bulletInteractable)
        {
            if (collision.gameObject.CompareTag(tag))
            {
                Collided = true;

                //Particle
                Vector2 hitPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
                PoolManager.SpawnObject(Bullet_Collision, hitPoint, Quaternion.identity, PoolManager.PoolType.ParticleSystem);

                //Detach trail before Pooling 
                var trail = GetComponent<TrailRenderer>();
                if(trail != null)
                {
                    trail.transform.parent = null;
                    // just taking the reference to avoid returning null if bullet is reused
                    var detachedTrail = trail;
                    effectsManager.RunCoroutine(ReturnTrailAfterTime(detachedTrail, detachedTrail.time));
                }

                //reset before returning
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.simulated = false;
                rb.Sleep();

                
                PoolManager.ReturnObjectToPool(gameObject, PoolManager.PoolType.GameObjects);
                break;
            }
        }

    }

    IEnumerator DisableBulletAfterSeconds(float time)
    {
        yield return new WaitForSeconds(time);

        if(!Collided && gameObject.activeInHierarchy)
            PoolManager.ReturnObjectToPool(gameObject, PoolManager.PoolType.GameObjects);
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
}
