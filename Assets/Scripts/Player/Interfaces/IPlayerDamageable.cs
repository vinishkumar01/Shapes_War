using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerDamageable
{
    void Damage(int damageAmount, Vector2 hitDirection);

    void Die();

    int MaxHealth { get; set; }

    int CurrentHealth { get; set; }   
}
