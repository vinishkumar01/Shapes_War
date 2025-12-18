using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class HealthBar : MonoBehaviour
{
    private Image _image;
    [SerializeField] private float _timeToDrain = 0.5f;
    [SerializeField] private Gradient _healthBarGradient;

    private float _target = 1f;

    private Coroutine drainHealthBarCoroutine;

    private Color _newHealthBarColor;

    private void Start()
    {
        _image = GetComponent<Image>();
        _image.color = _healthBarGradient.Evaluate(_target);

        CheckHealthBarGradientColorAmount();
    }

    public void UpdateHealthBar(float maxHealth, float currentHealth)
    {
        if(!gameObject.activeInHierarchy) return;

        _target = currentHealth / maxHealth ;

        drainHealthBarCoroutine = StartCoroutine(DrainHealth());

        CheckHealthBarGradientColorAmount() ;
    }


    private IEnumerator DrainHealth()
    {
        float fillamount = _image.fillAmount;
        Color currentColor = _image.color;

        float elaspsedTime = 0f;

        while(elaspsedTime < _timeToDrain)
        {
            elaspsedTime += Time.deltaTime;

            //Lerp the fill amount 
            _image.fillAmount = Mathf.Lerp(fillamount, _target, (elaspsedTime / _timeToDrain));

            //lerp the color based on the gradient 
            _image.color = Color.Lerp(currentColor, _newHealthBarColor, (elaspsedTime / _timeToDrain));

            yield return null;   
        }
    }


    void CheckHealthBarGradientColorAmount()
    {
       _newHealthBarColor = _healthBarGradient.Evaluate(_target);
    }
}
