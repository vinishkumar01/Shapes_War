using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable 
{
    public void RecieveHit(RaycastHit2D RayHit);

    void Die();

    int MaxHealth { get; set; }

    int CurrentHealth { get; set; }   

    int DamageAmount { get; set; }
}
