using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
   public void OnExplosionAnimationEnd()
    {
        PoolManager.ReturnObjectToPool(gameObject, PoolManager.PoolType.GameObjects);
    }
}
