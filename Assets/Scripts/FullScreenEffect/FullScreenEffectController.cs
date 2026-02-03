using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullScreenEffectController : MonoBehaviour
{
    [Header("Time Stats")]
    [SerializeField] private float _lowHealthFadeOutTime = 0.5f;

    [Header("Reference")]
    [SerializeField] private ScriptableRendererFeature _fullScreenLowHealthEffect;
    [SerializeField] private Material _material;

    private int _vignettePower = Shader.PropertyToID("_VignettePower");
    private int _vignetteIntensity = Shader.PropertyToID("_VignetteIntensity");

    [SerializeField] private float _vignettePowerAmount = 5.15f;
    [SerializeField] private float _vignetteIntensityAmount = 1.4f;

    private float _defaultVignetteIntensity = 1.25f;
    private float _defaultVignettePower = 6.65f;

    //Condition
    public bool _lowHealthEffectActive = false;

    //Coroutine Reference
    public Coroutine lowHealthEffectCoroutine;

    

    private void Start()
    {
        _fullScreenLowHealthEffect.SetActive(false);
    }

    public IEnumerator LowHealthEffect()
    {
        _fullScreenLowHealthEffect.SetActive(true);
        
        while(_lowHealthEffectActive)
        {
            //Lerp Up
            yield return LerpVignette(_defaultVignetteIntensity, _vignetteIntensityAmount, _defaultVignettePower, _vignettePowerAmount, _lowHealthFadeOutTime);

            if(!_lowHealthEffectActive)
            {
                yield break;
            }

            //Lerp Down (From Final value to default value)
            yield return LerpVignette(_vignetteIntensityAmount, _defaultVignetteIntensity, _vignettePowerAmount, _defaultVignettePower, _lowHealthFadeOutTime);
        }
    }

    private IEnumerator LerpVignette(float startIntensity, float endIntensity, float startPower, float endPower, float duration)
    {
        float t = 0f;

        while(t < duration)
        {
            t += Time.deltaTime;

            float lerp = t / duration;
            lerp = Mathf.SmoothStep(0f,1f, lerp);

            _material.SetFloat(_vignetteIntensity, Mathf.Lerp(startIntensity, endIntensity, lerp));
            _material.SetFloat(_vignettePower, Mathf.Lerp(startPower, endPower, lerp));

            yield return null;
        }
    }


    public void ResetLowHealthEffect()
    {
        _material.SetFloat(_vignetteIntensity, _defaultVignetteIntensity);
        _material.SetFloat(_vignettePower, _defaultVignettePower);
        _fullScreenLowHealthEffect.SetActive(false);
    }

    private void OnApplicationQuit()
    {
        OnDestroy();
    }

    private void OnDestroy()
    {
        ResetLowHealthEffect();
    }
}
