using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HM_Explosion : MonoBehaviour
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        StartCoroutine(waitForAnimationThenReturn());
    }

    IEnumerator waitForAnimationThenReturn()
    {
        string animStateName = "Explosion";

        while (!_animator.GetCurrentAnimatorStateInfo(0).IsName(animStateName))
            yield return null;

        while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;

        PoolManager.ReturnObjectToPool(gameObject, PoolManager.PoolType.GameObjects);
    }
}
