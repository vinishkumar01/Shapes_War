using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPartsAutoReturn : MonoBehaviour
{
    [SerializeField] private float _lifeTime = 3f;

    private void OnEnable()
    {
        StartCoroutine(ReturnBodyToPool());
    }

    private IEnumerator ReturnBodyToPool()
    {
        yield return new WaitForSeconds(_lifeTime);

        PoolManager.ReturnObjectToPool(this.gameObject, PoolManager.PoolType.GameObjects);
    }
}
