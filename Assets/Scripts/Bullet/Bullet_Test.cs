using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet_Test : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] LayerMask bulletInteractables;

    [Header("Bullet Configs")]
    [SerializeField] int bulletSpeed = 50;
    [SerializeField] Vector3 SpawnPosition;
    [SerializeField] Vector3 MousePosition;
    [SerializeField] Vector3 lastPosition;
    [SerializeField] float bulletMaxTravelDistance = 30;

    [Header("Effects")]
    [SerializeField] ParticleSystem bullet_Collision;

    private void OnEnable()
    {
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();   
    }

    private void Start()
    {
        SpawnPosition = rb.position;
        lastPosition = SpawnPosition;
        MousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 Direction = (MousePosition - SpawnPosition).normalized;

        rb.velocity = Direction * bulletSpeed;
    }

    private void FixedUpdate()
    {
        Vector3 currentPosition = rb.position;
        Vector3 direction = (currentPosition - lastPosition).normalized;
        float distance = direction.magnitude;

        if(distance > 0)
        {
            //Debug.DrawLine(lastPosition, currentPosition, Color.yellow, 0.1f);
            RaycastHit2D hit = Physics2D.Raycast(lastPosition, direction, distance, bulletInteractables);

            if (hit.collider != null)
            {
                Debug.Log($"Bullet hit: {hit.collider.name} | Tag: {hit.collider.tag}");

                if(hit.collider.CompareTag("Platform") || hit.collider.CompareTag("Enemy"))
                {
                    Debug.Log("Hit");
                    //HandleCollision(hit.collider, hit.point);
                }
            }
            else if(hit.collider == null && Vector3.Distance(SpawnPosition, transform.position) > bulletMaxTravelDistance)
            {
                 PoolManager.ReturnObjectToPool(gameObject, PoolManager.PoolType.GameObjects);
            }
        }

        lastPosition = currentPosition;
    }

   void HandleCollision(Collider2D collider, Vector2 hitPoint)
    {
        PoolManager.ReturnObjectToPool(gameObject, PoolManager.PoolType.GameObjects);

        PoolManager.SpawnObject(bullet_Collision, hitPoint, Quaternion.identity, PoolManager.PoolType.ParticleSystem);
    }
}
