using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodDroplet : MonoBehaviour
{
    private Vector2 _velocity;
    private float _lifeTime;

    private const float _gravity = 8f;
    private const float _drag = 0.98f;
    private const float _rayCast_padding = 0.02f;

    private bool _isReturning;

    [SerializeField] private LayerMask _layerMask;

    private BloodStainSpawnManager.CharacterType _characterType;

    private void OnEnable()
    {
        _isReturning = false;
    }

    public void Init(Vector2 startPos, Vector2 initialVelocity, BloodStainSpawnManager.CharacterType characterType)
    {
        transform.position = startPos;
        _velocity = initialVelocity;
        _lifeTime = Random.Range(0.8f, 1.5f);
        _characterType = characterType;
    }

    private void Update()
    {
        _lifeTime -= Time.deltaTime;

        if (_lifeTime <= 0f )
        {
            ReturnToPool();
            return;
        }

        Collider2D overlap = Physics2D.OverlapPoint(transform.position, _layerMask);

        if (overlap != null)
        {
            Vector2 normal = ((Vector2)transform.position - (Vector2)overlap.bounds.center).normalized;

            SpawnStain(transform.position, normal);
            ReturnToPool();
            return;
        }

        Vector2 pos = transform.position;
        Vector2 step = _velocity * Time.deltaTime;

        RaycastHit2D hit = Physics2D.Raycast(pos, _velocity.normalized, step.magnitude + _rayCast_padding, _layerMask);

        if(hit.collider != null)
        {
            SpawnStain(hit.point, hit.normal);
            ReturnToPool();
            return;
        }

        _velocity += Vector2.down * _gravity * Time.deltaTime;
        _velocity *= _drag;

        transform.position = pos + step;
    }

    private void SpawnStain(Vector2 point, Vector2 normal)
    {
        BloodStainSpawnManager.instance.Spawn(point, normal, _characterType);
    }

    private void ReturnToPool()
    {
        if(_isReturning)
        {
            return;
        }
        _isReturning = true;
        
        PoolManager.ReturnObjectToPool(gameObject, PoolManager.PoolType.BloodDroplet);
    }
}
