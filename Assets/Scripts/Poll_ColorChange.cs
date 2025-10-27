using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poll_ColorChange : MonoBehaviour, IHittable
{
    [SerializeField] SpriteRenderer sprite;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        StartCoroutine(ChangeColor());

    }


    void IHittable.RecieveHit(RaycastHit2D RayHit)
    {
        this.sprite.color = Color.red;
    }

    IEnumerator ChangeColor()
    {
        this.sprite.color = Color.white;
        yield return null;
    }
}
