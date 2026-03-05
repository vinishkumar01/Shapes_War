using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicLampbehaviour : MonoBehaviour, IDamageable, IUpdateObserver
{
    [Header("Reference")]
    [SerializeField] private ParticleSystem _lightImpactCollision;

    [Header("Light Attributes")]
    [SerializeField] private int _maxHealth = 20;
    [SerializeField] private int _damageAmount = 10;

    public int MaxHealth { get; set; }
    public int CurrentHealth { get; set; }
    public int DamageAmount { get; set; }

    private void OnEnable()
    {
        UpdateManager.RegisterObserver(this);

        //Assign health 
        MaxHealth = _maxHealth;
        DamageAmount = _damageAmount;

        CurrentHealth = MaxHealth;
    }

    public void ObservedUpdate()
    {

    }

    public void RecieveHit(RaycastHit2D RayHit, Vector2 hitDirection)
    {
        if (CurrentHealth <= 0) return;

        CurrentHealth -= DamageAmount;


        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        //Spawn Light shatter effect
        PoolManager.SpawnObject(_lightImpactCollision, transform.position, Quaternion.identity, PoolManager.PoolType.ParticleSystem);

        gameObject.SetActive(false);
    }


    private void OnDisable()
    {
        UpdateManager.UnregisterObserver(this);
    }
}
