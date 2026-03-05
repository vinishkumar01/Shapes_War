using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodStainLife : MonoBehaviour, IUpdateObserver
{
    private float _life;
    private float _fadeStart;
    private SpriteRenderer _sr;

    private Color _startColor;
    private Color _dryColor;

    private void OnEnable()
    {
        UpdateManager.RegisterObserver(this);

        _sr = GetComponent<SpriteRenderer>();

        _life = Random.Range(12f, 25f);
        _fadeStart = _life * 0.6f; //Start fading at 60%

        _startColor = _sr.color;

        Color driedBloodTarget = new Color(0.08f, 0.15f, 0.45f, _startColor.a);
        _dryColor = Color.Lerp(_startColor, driedBloodTarget, 0.6f); //darker and dried

        gameObject.SetActive(true);

        transform.localScale = transform.localScale;
    }


    public void ObservedUpdate()
    {
        _life -= Time.deltaTime;

        if(_life <= 0f)
        {
            ReturnToPool();
            return;
        }

        if(_life <= _fadeStart)
        {
            float t = 1f - (_life / _fadeStart);
            Color c = Color.Lerp(_startColor, _dryColor, t);
            c.a = Mathf.Lerp(_startColor.a, 0.5f, t); //little bit of add on the color alpha fade
            _sr.color = c;
        }
    }

    private void ReturnToPool()
    {
        BloodStainSpawnManager.instance.OnStainReturned();
        PoolManager.ReturnObjectToPool(gameObject, PoolManager.PoolType.BloodStains);
    }

    private void OnDisable()
    {
        UpdateManager.UnregisterObserver(this);
    }
}
