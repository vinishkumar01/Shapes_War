using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poll_ColorChange : MonoBehaviour, IHittable
{
    [SerializeField] SpriteRenderer sprite;
    private FlashEffect _flashEffect;
    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        _flashEffect = GetComponent<FlashEffect>();
        StartCoroutine(ChangeColor());

    }


    void IHittable.RecieveHit(RaycastHit2D RayHit)
    {
        _flashEffect.CallDamageFlash();
    }


    IEnumerator ChangeColor()
    {
        this.sprite.color = Color.white;
        yield return null;
    }
}
