using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockBack : MonoBehaviour
{
    [SerializeField] float knockBackTime = 0.2f;
    [SerializeField] float hitDirectionForce = 10f;
    [SerializeField] float constForce = 5f;
    [SerializeField] float inputForce = 7f;

    Rigidbody2D rb;

    public bool IsBeingKnockedBack { get; private set; }

    public Coroutine knockBackCoroutine;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public IEnumerator KnockBackAction(Vector2 hitDirection, Vector2 constantForceDirection, float inputDirection)
    {
        IsBeingKnockedBack = true;

        Vector2 _hitForce;
        Vector2 _constForce;
        Vector2 _knockBackForce;
        Vector2 _combinedForce;

        _hitForce = hitDirection * hitDirectionForce;
        _constForce  = constantForceDirection * constForce;

        float elapsedTime = 0f;
        while (elapsedTime < knockBackTime)
        {
             elapsedTime += Time.fixedDeltaTime;

            //combine _hitForce and _constForce
            _knockBackForce = _hitForce + _constForce; 

            //combine knockBackForce with inputDirection
            if(inputDirection != 0)
            {
                _combinedForce = _knockBackForce + new Vector2(inputDirection * inputForce,0);
            }
            else
            {
                _combinedForce = _knockBackForce; 
            }
            
            //Apply KnockBack to the rigidBody

            rb.velocity = _combinedForce;

            yield return new WaitForFixedUpdate();

        }

        IsBeingKnockedBack = false;
    }

    public void callKnockBackCoroutine(Vector2 hitDirection, Vector2 constantForceDirection, float inputDirection)
    {
        knockBackCoroutine = StartCoroutine(KnockBackAction(hitDirection, constantForceDirection, inputDirection));
    }
}
