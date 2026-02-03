using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudNearMovement : MonoBehaviour, IUpdateObserver
{
    [SerializeField] private float _speed = 0.02f;
    [SerializeField] private float _tileWorldWidth;   

    private SpriteRenderer _spriteRenderer;
    private float _startX;

    private void OnEnable()
    {
        UpdateManager.RegisterObserver(this);
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _spriteRenderer.drawMode = SpriteDrawMode.Tiled;

        _tileWorldWidth = _spriteRenderer.bounds.size.x * transform.localScale.x;
    }


    public void ObservedUpdate()
    {
        transform.position += Vector3.right * _speed * Time.deltaTime;

        if (transform.position.x >= _tileWorldWidth)
        {
            transform.position -= new Vector3(_tileWorldWidth, 0f, 0f);
        }
    }

    private void OnDisable()
    {
        UpdateManager.UnregisterObserver(this);
    }
}
