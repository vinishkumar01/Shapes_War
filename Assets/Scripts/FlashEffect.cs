using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashEffect : MonoBehaviour
{
    [ColorUsage(true, true)]
    [SerializeField] Color _flashColor = Color.red;
    [SerializeField] float flashTime = 0.25f;
    [SerializeField] private AnimationCurve _flashSpeedCurve;

    private SpriteRenderer _spriteRenderer;
    private Material _material;

    private Coroutine damageFlashCoroutine;
    

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _material = _spriteRenderer.material;
    }


    public void CallDamageFlash()
    {
        if(this.isActiveAndEnabled)
        {
            if(damageFlashCoroutine != null)
                StopCoroutine(damageFlashCoroutine);
            damageFlashCoroutine = StartCoroutine(DamageFlasher());
        }
    }

    private IEnumerator DamageFlasher()
    {
        // Set the Color 
        SetFlashColor();

        // lerp the flash amount
        float currentflashAmount = 0f;
        float elapsedTime = 0f;


        while (elapsedTime < flashTime) 
        {
            //iterate elapsedTime
            elapsedTime += Time.deltaTime;

            //lerp the flash amount
            currentflashAmount = Mathf.Lerp(1f, _flashSpeedCurve.Evaluate(elapsedTime), (elapsedTime / flashTime));

            //
            SetFlashAmount(currentflashAmount);

            yield return null;
        }
    }

     private void SetFlashColor()
     {    
        _material.SetColor("_flashColor", _flashColor);
     }

    private void SetFlashAmount(float amount)
    {
        //Set the flash Amount
        _material.SetFloat("_flashAmount", amount);
    }

    public void ResetFlash()
    {
        SetFlashAmount(0f);
    }
}
