using System.Collections;
using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    Vector3 _startposition;
    Vector3 _targetposition;
    float _distance;
    float progress;
    public bool ScriptIsBeingCalled = true;

    [Header("Tracer Cofigs")]
    [SerializeField] float _speed = 80f;
    [SerializeField] float _trialLife = 0.1f;
    [SerializeField] GameObject _bulletCollision;
    [SerializeField] GameObject _underGroundBulletCollision;
    [SerializeField] bool hasHit = false;
    bool hasBeenReturnedToPool = false;

    [Header("SFX")]
    [SerializeField] private AudioClip _collisionEffect;

    RaycastHit2D storedHit;

    private void OnEnable()
    {
        hasHit = false;
        hasBeenReturnedToPool = false;
        progress = 0f;
    }

    public void initialize(Vector3 startPosition, Vector3 targetPosition, RaycastHit2D hit)
    {
        _startposition = new Vector3(startPosition.x, startPosition.y, -1);
        _targetposition = new Vector3(targetPosition.x, targetPosition.y, -1);
        _distance = Vector3.Distance(_startposition, _targetposition);

        progress = 0f;
        transform.position = _startposition;

        storedHit = hit;
        hasHit = false;
        hasBeenReturnedToPool = false;
    }

    void Update()
    {
        if (!hasHit && storedHit.collider != null)
        {
            
            if (storedHit.collider.TryGetComponent<IDamageable>(out var damageable))
            {
                Vector3 worldHitPoint = new(storedHit.collider.transform.position.x, storedHit.collider.transform.position.y, -1);
                _targetposition = new Vector3(worldHitPoint.x, worldHitPoint.y, -1);
                _distance = Vector3.Distance(_startposition, _targetposition);
            }

        }

        progress += (_speed * Time.deltaTime) / _distance;
        transform.position = Vector3.Lerp(_startposition, _targetposition, progress);

        if (progress >= 1f && !hasHit)
        {
            hasHit = true;
            if (storedHit.collider != null)
            {

                if(storedHit.collider.CompareTag("Platform") || storedHit.collider.CompareTag("PlatformwithNoNodes") || storedHit.collider.CompareTag("UnderGroundPlatform"))
                {
                    // We spawn the particle slightly outward of the platform so that the collision doesnt affects between particle and platform
                    Vector3 fxpos = _targetposition + (Vector3)(storedHit.normal * 0.2f);

                    if(storedHit.collider.CompareTag("UnderGroundPlatform"))
                    {
                        SpawnDustParticle(fxpos, storedHit.normal, _underGroundBulletCollision);
                    }
                    else
                    {
                        SpawnDustParticle(fxpos, storedHit.normal, _bulletCollision);
                    }

                    //StartCoroutine(ReturnAfterSeconds(impact, 1f));

                    //Play Collision Sound Effect
                    SFXManager._instance.playSFX(_collisionEffect, fxpos, 1f, true, false);
                }

                if (storedHit.collider.gameObject.activeInHierarchy && storedHit.collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    Vector2 hitDirection = (_targetposition - _startposition).normalized;
                    damageable.RecieveHit(storedHit, hitDirection);
                }
                StartCoroutine(DisableWhenHit());
            }
            else
            {
                StartCoroutine(DisableAfterTrail());
            }
        }
    }


    private void SpawnDustParticle(Vector2 hitPoint, Vector2 hitNormal, GameObject collisionEffect)
    {
        var dustParticle = PoolManager.SpawnObject(collisionEffect, hitPoint, Quaternion.identity, PoolManager.PoolType.ParticleSystem).GetComponent<ParticleSystem>();

        float zRotation;

        //Check the ceiling and floor of the platform
        if(Mathf.Abs(hitNormal.x) > Mathf.Abs(hitNormal.y))
        {
            //wall
            zRotation = hitNormal.x > 0 ? -90f : 90f;
        }
        else
        {
            //floor / ceiling
            zRotation = hitNormal.y > 0 ? 180f : 0f;
        }

        dustParticle.transform.rotation = Quaternion.Euler(0, 0, zRotation);

        dustParticle.Play();
    }
    private void ReturnToPoolOnce()
    {
        if (hasBeenReturnedToPool) return;
        hasBeenReturnedToPool = true;
        PoolManager.ReturnObjectToPool(gameObject, PoolManager.PoolType.GameObjects);
    }

    IEnumerator DisableAfterTrail()
    {
        yield return new WaitForSeconds(_trialLife + 1.5f);
        ReturnToPoolOnce();
    }

    public IEnumerator DisableWhenHit()
    {
        yield return new WaitForSeconds(_trialLife + 0.1f);
        ReturnToPoolOnce();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
