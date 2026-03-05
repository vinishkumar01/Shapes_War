using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFlashAndDissolveEffect : MonoBehaviour
{
    [ColorUsage(true, true)]
    [SerializeField] Color _flashColor = Color.red;
    [SerializeField] float flashTime = 0.25f;
    [SerializeField] private AnimationCurve _flashSpeedCurve;

    [Header("Player Visual SpriteRenderer")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    private Material _material;

    public Coroutine damageFlashCoroutine;

    [Header("Dash Flash Color")]
    [ColorUsage(true, true)]
    [SerializeField] Color _dashFlashColor;

    public Coroutine _dashFlashCoroutine;

    [Header("Dissolve Attributes")]
    [SerializeField] private float _dissolveTime = 0.75f;

    private int _dissolveAmount = Shader.PropertyToID("_DissolveAmount");
    private int _verticalDissolveAmount = Shader.PropertyToID("_VerticalDissolve");

    public Coroutine _dissolveCoroutine;

    private void Awake()
    {
        _material = _spriteRenderer.material;
    }

    #region Damage Flash
    //Damage Flash
    public void CallDamageFlash()
    {
        if (this.isActiveAndEnabled)
        {
            if (damageFlashCoroutine != null)
            {
                StopCoroutine(damageFlashCoroutine);
            }

            damageFlashCoroutine = StartCoroutine(DamageFlasher());
        }
    }

    public void StopFlashEffect()
    {
        if (damageFlashCoroutine != null)
        {
            StopCoroutine(damageFlashCoroutine);
            damageFlashCoroutine = null;
        }
        ResetFlash();
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
        _material.SetColor("_FlashColor", _flashColor);
    }

    private void SetFlashAmount(float amount)
    {
        //Set the flash Amount
        _material.SetFloat("_FlashAmount", amount);
    }

    public void ResetFlash()
    {
        SetFlashAmount(0f);
    }

    #endregion

    #region Dash Flash
    //Dash Flash
    public void CallDashFlash()
    {
        if (this.isActiveAndEnabled)
        {
            if (_dashFlashCoroutine != null)
            {
                StopCoroutine(_dashFlashCoroutine);
            }

            _dashFlashCoroutine = StartCoroutine(DashFlasher());
        }
    }

    public void StopDashFlashEffect()
    {
        if (_dashFlashCoroutine != null)
        {
            StopCoroutine(_dashFlashCoroutine);
            _dashFlashCoroutine = null;
        }
        ResetFlash();
    }

    private IEnumerator DashFlasher()
    {
        // Set the Color 
        SetDashFlashColor();

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

    private void SetDashFlashColor()
    {
        _material.SetColor("_FlashColor", _dashFlashColor);
    }

    #endregion

    #region Respawn Dissolve Effect

    public void CallDissolveEffect()
    {
        if (this.isActiveAndEnabled)
        {
            if (_dissolveCoroutine != null)
            {
                StopCoroutine(_dissolveCoroutine);
            }

            _dissolveCoroutine = StartCoroutine(Appear(false, true));
        }
    }

    public void StopDissolveEffect()
    {
        if (_dissolveCoroutine != null)
        {
            StopCoroutine(_dissolveCoroutine);
            _dissolveCoroutine = null;
        }
    }

    private IEnumerator Appear(bool useDissolve, bool usevertical)
    {
        float elapsedTime = 0f;

        while(elapsedTime < _dissolveTime)
        {
            elapsedTime += Time.deltaTime;

            float lerpedDissolve = Mathf.Lerp(1.1f, 0f, (elapsedTime / _dissolveTime));
            float lerpedVertical = Mathf.Lerp(1.1f, 0f, (elapsedTime / _dissolveTime));

            if(useDissolve)
            {
                _material.SetFloat(_dissolveAmount, lerpedDissolve);
            }

            if(usevertical)
            {
                _material.SetFloat(_verticalDissolveAmount, lerpedVertical);
            }

            yield return null;
        }
    }
    #endregion
}
