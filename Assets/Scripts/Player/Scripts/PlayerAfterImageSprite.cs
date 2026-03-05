using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterImageSprite : MonoBehaviour, IUpdateObserver
{
    [Header("Time")]
    [SerializeField] private float _activeTime = 0.1f; // To keep on track of how long this gameObject to be active and also to 
    private float _timeActivated; //keep track of how long it was active

    [Header("alpha")]
    private float _alpha; // We also need to track of what current alpha value is!
    [SerializeField] private float _setAlpha = 0.8f; // so this is where we will set the alpha value for the sprite of the gameobject when we are enabling
    [SerializeField] private float _alphaDecay = 0.85f; // To decrease the alpha value overTime, The greater the number the faster it fades away

    [Header("Reference")]
    private Transform _playerVisualTransform;
    private SpriteRenderer _sr;
    private SpriteRenderer _playerVisualSR;

    private Color _color;


    private void OnEnable()
    {
        _playerVisualTransform = GameObject.FindGameObjectWithTag("PlayerVisuals").transform;

        _sr = GetComponent<SpriteRenderer>();
        _playerVisualSR = _playerVisualTransform.GetComponent<SpriteRenderer>();

        _alpha = _setAlpha;
        _sr.sprite = _playerVisualSR.sprite; // get the sprite of the player and assign to this sprite
        //_sr.color = Color.blue; //This line doesnt work as expected but it looks good, i tried to make the after image blue but as we are setting the color alpha _color = new Color(1f, 1f, 1f, _alpha). this line is not gonna effective as it becomes white, but at the start its blue and it looks good
        transform.position = _playerVisualTransform.position;
        transform.rotation = _playerVisualTransform.rotation;
        transform.localScale = new Vector2(_playerVisualTransform.transform.localScale.x + 0.1f, _playerVisualTransform.transform.localScale.y + 0.2f);
        _timeActivated = Time.time;

        //Register 
        UpdateManager.RegisterObserver(this);
    }

    public void ObservedUpdate()
    {
        _alpha -= _alphaDecay * Time.deltaTime;
        _color = new Color(1f, 1f, 1f, _alpha);
        _sr.color = _color;

        if(Time.time >= (_timeActivated + _activeTime))
        {
            PoolManager.ReturnObjectToPool(gameObject, PoolManager.PoolType.PlayerAfterimage);
        }
    }

    private void OnDisable()
    {
        UpdateManager.UnregisterObserver(this);
    }
}
