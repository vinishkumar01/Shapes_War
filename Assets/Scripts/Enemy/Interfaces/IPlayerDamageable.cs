using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerDamageable
{
    void Damage(float damageAmount, Vector2 hitDirection);

    void Die();

    float MaxHealth { get; set; }

    float CurrentHealth { get; set; }   
}
