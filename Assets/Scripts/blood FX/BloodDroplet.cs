using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodDroplet : MonoBehaviour
{
    Vector2 _velocity;
    float _lifeTime;

    const float _gravity = 8f;
    const float _drag = 0.98f;
    const float _rayCast_padding = 0.02f;

    [SerializeField] private LayerMask _layerMask;

    public void Init(Vector2 startPos, Vector2 initialVelocity)
    {
        transform.position = startPos;
        _velocity = initialVelocity;
        _lifeTime = Random.Range(0.8f, 1.5f);
    }

    private void Update()
    {
        _lifeTime -= Time.deltaTime;

        if (_lifeTime <= 0f )
        {
            Destroy(gameObject);
            return;
        }


        Vector2 pos = transform.position;
        Vector2 step = _velocity * Time.deltaTime;

        RaycastHit2D hit = Physics2D.Raycast(pos, _velocity.normalized, step.magnitude + _rayCast_padding, _layerMask);

        if(hit.collider != null)
        {
            SpawnStain(hit.point, hit.normal);
            PoolManager.ReturnObjectToPool(gameObject);
        }

        _velocity += Vector2.down * _gravity * Time.deltaTime;
        _velocity *= _drag;

        transform.position = pos + step;
    }

    private void SpawnStain(Vector2 point, Vector2 normal)
    {
        if(_velocity.magnitude < 1.5f)
            return;

        BloodStainSpawnManager.instance.Spawn(point, normal);
    }
}
