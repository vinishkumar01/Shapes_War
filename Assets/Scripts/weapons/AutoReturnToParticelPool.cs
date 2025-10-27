using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoReturnToParticelPool : MonoBehaviour
{
    private ParticleSystem particlesys;

    private bool isReturning = false;

    private void Awake()
    {
        particlesys = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        isReturning = false;
        if(particlesys == null)
        {
            particlesys= GetComponent<ParticleSystem>();
        }
    }

    private void Update()
    {
        if(!isReturning && particlesys != null && !particlesys.IsAlive(true))
        {
            isReturning = true;
            PoolManager.ReturnObjectToPool(gameObject, PoolManager.PoolType.ParticleSystem);
        }
    }
}
