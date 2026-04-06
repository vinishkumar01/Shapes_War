using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ReturnPowerUpsAfterFewSeconds : MonoBehaviour
{
    [SerializeField] private float _returnTime = 10f;
    private Coroutine _returnCoroutine;

    private void OnEnable()
    {
        _returnCoroutine = StartCoroutine(ReturnToPool());
    }

    private void OnDisable()
    {
        if(_returnCoroutine != null)
        {
            StopCoroutine(_returnCoroutine);
        }
    }

    private IEnumerator ReturnToPool()
    {
        yield return new WaitForSeconds(_returnTime);

        PoolManager.ReturnObjectToPool(this.gameObject, PoolManager.PoolType.GameObjects);
    }
}
