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

    [SerializeField] float _speed = 80f;
    [SerializeField] float _trialLife = 0.1f;
    [SerializeField] GameObject Bullet_Collision;
    [SerializeField] bool hasHit = false;

    RaycastHit2D storedHit;

    void Start()
    {
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
    }

    void Update()
    {
        if(!hasHit && storedHit.collider != null)
        {
            var hittable = storedHit.collider.GetComponent<IHittable>();

            if(hittable != null )
            {
                _targetposition = new Vector3(storedHit.collider.transform.position.x, storedHit.collider.transform.position.y, -1);
                _distance = Vector3.Distance(_startposition, _targetposition);
            }
            
        }
        
        progress += (_speed * Time.deltaTime) / _distance;
        transform.position = Vector3.Lerp(_startposition, _targetposition, progress);

        if(progress >= 1f && !hasHit)
        {
            hasHit = true;
            if(storedHit.collider != null)
            {
                GameObject impact = PoolManager.SpawnObject(Bullet_Collision, _targetposition, Quaternion.identity, PoolManager.PoolType.ParticleSystem);
                StartCoroutine(ReturnAfterSeconds(impact, 1f));

                var hittable = storedHit.collider.GetComponent<IHittable>();
                if (hittable != null)
                {
                    hittable.RecieveHit(storedHit);
                    StartCoroutine(DisableWhenHit());
                }
            }

            //StopAllCoroutines();
            StartCoroutine(DisableAfterTrail());
        }
    }

    IEnumerator DisableAfterTrail()
    {
        yield return new WaitForSeconds(_trialLife + 1.5f);
        PoolManager.ReturnObjectToPool(gameObject);
    }

    public IEnumerator DisableWhenHit()
    {
        yield return new WaitForSeconds(_trialLife + 0.1f);
        PoolManager.ReturnObjectToPool(gameObject);
    }

    IEnumerator ReturnAfterSeconds(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        PoolManager.ReturnObjectToPool(obj, PoolManager.PoolType.ParticleSystem);
    }
}
