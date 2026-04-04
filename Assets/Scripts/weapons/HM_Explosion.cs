using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class HM_Explosion : MonoBehaviour
{
    private Animator _animator;
    private Light2D _light;

    [Header("Glow Settings")]
    [SerializeField] private float maxIntensity = 3f;
    [SerializeField] private float outerRadius = 2f;
    [SerializeField] private Color glowColor = new Color(1f, 0.5f, 0f); // orange

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        // Add Light2D dynamically
        _light = gameObject.AddComponent<Light2D>();
        _light.lightType = Light2D.LightType.Point;
        _light.pointLightOuterRadius = outerRadius;
        _light.color = glowColor;
        _light.intensity = 0f;
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
        {
            float progress = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

            // Peaks at 30% of animation then fades out
            float intensity = Mathf.Sin(progress * Mathf.PI) * maxIntensity;
            _light.intensity = intensity;

            yield return null;
        }

        _light.intensity = 0f;
        PoolManager.ReturnObjectToPool(gameObject, PoolManager.PoolType.GameObjects);
    }
}
