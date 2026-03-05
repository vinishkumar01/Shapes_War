using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockWaveBehaviour : MonoBehaviour
{
    [SerializeField] private float _shockWaveTime = 0.75f;
    [SerializeField] private float _shockWaveColliderTime = 0.74f;

    private Coroutine _shockWaveCoroutine;

    private Material _material;

    private static int _waveDistanceFromCenter = Shader.PropertyToID("_WaveDistanceFromCenter");

    private CircleCollider2D _circleCollider;

    private void Awake()
    {
        _material = GetComponent<SpriteRenderer>().material;
        _circleCollider = GetComponent<CircleCollider2D>();
    }


    public void CallShockWave()
    {
        _shockWaveCoroutine = StartCoroutine(ShockWaveAction(-0.1f, 1f, 0.01f, 0.28f)); //(wave Start and End Pos, Collider Start and End Pos)
    }

    //private void Update()
    //{
    //    if(Input.GetKeyDown(KeyCode.E))
    //    {
    //        CallShockWave();
    //    }
    //}

    private IEnumerator ShockWaveAction(float waveStartPos, float WaveEndPos, float colliderStartPos, float colliderEndPos)
    {

        _material.SetFloat(_waveDistanceFromCenter, waveStartPos);

        _circleCollider.radius = colliderStartPos;

        float waveLerpedAmount = 0f;
        float colliderLerpedAmount = 0f;

        float elapsedTime = 0f;

        while(elapsedTime < _shockWaveTime)
        {
            elapsedTime += Time.deltaTime;

            //Lerping the wave
            waveLerpedAmount = Mathf.Lerp(waveStartPos, WaveEndPos, (elapsedTime / _shockWaveTime));
            _material.SetFloat(_waveDistanceFromCenter, waveLerpedAmount);

            //Lerping the Collider to match the wave
            colliderLerpedAmount = Mathf.Lerp(colliderStartPos, colliderEndPos, (elapsedTime / _shockWaveColliderTime));
            _circleCollider.radius = colliderLerpedAmount;

            yield return null;
        }

        PoolManager.ReturnObjectToPool(this.gameObject, PoolManager.PoolType.ParticleSystem);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.TryGetComponent(out IDamageable damageable))
        {
            damageable.Die();
        }
        
    }
}
