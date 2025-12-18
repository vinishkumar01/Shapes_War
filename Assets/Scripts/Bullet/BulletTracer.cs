using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] GameObject Bullet_Collision;
    [SerializeField] bool hasHit = false;
    bool hasBeenReturnedToPool = false;

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
                GameObject impact = PoolManager.SpawnObject(Bullet_Collision, _targetposition, Quaternion.identity, PoolManager.PoolType.ParticleSystem);
                StartCoroutine(ReturnAfterSeconds(impact, 1f));

                if (storedHit.collider.gameObject.activeInHierarchy && storedHit.collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.RecieveHit(storedHit);
                }
                StartCoroutine(DisableWhenHit());
            }
            else
            {
                StartCoroutine(DisableAfterTrail());
            }
        }
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

    IEnumerator ReturnAfterSeconds(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        PoolManager.ReturnObjectToPool(obj, PoolManager.PoolType.ParticleSystem);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
