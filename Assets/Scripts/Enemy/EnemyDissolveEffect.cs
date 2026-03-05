using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDissolveEffect : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Enemy _enemy;

    [Header("Dissolve Attributes")]
    [SerializeField] private float _dissolveTime = 0.75f;

    private int _dissolveAmount = Shader.PropertyToID("_DissolveAmount");
    private int _verticalDissolveAmount = Shader.PropertyToID("_VerticalDissolve");

    public Coroutine _dissolveCoroutine;


    private void Awake()
    {
        _enemy = GetComponentInParent<Enemy>();
    }

    #region spawn Dissolve Effect

    public void CallDissolveEffect()
    {
        if (this.isActiveAndEnabled)
        {
            if (_dissolveCoroutine != null)
            {
                StopCoroutine(_dissolveCoroutine);
            }

            _dissolveCoroutine = StartCoroutine(Appear(true, false));
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

        while (elapsedTime < _dissolveTime)
        {
            elapsedTime += Time.deltaTime;

            float lerpedDissolve = Mathf.Lerp(1.1f, 0f, (elapsedTime / _dissolveTime));
            float lerpedVertical = Mathf.Lerp(1.1f, 0f, (elapsedTime / _dissolveTime));

            if (useDissolve)
            {
                _enemy._enemyVisualMaterial.SetFloat(_dissolveAmount, lerpedDissolve);
            }

            if (usevertical)
            {
                _enemy._enemyVisualMaterial.SetFloat(_verticalDissolveAmount, lerpedVertical);
            }

            yield return null;
        }
    }
    #endregion
}
