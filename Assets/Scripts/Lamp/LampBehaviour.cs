using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampBehaviour : MonoBehaviour, IDamageable
{
    [Header("Reference")]
    [SerializeField] private GameObject _lightPoll;
    [SerializeField] private GameObject _lightBulb;
    [SerializeField] private GameObject _light;
    [SerializeField] private BoxCollider2D _lightCollider;
    [SerializeField] private ParticleSystem _lightImpactCollision;

    [Header("Light Attributes")]
    [SerializeField] private int _maxHealth = 10;
    [SerializeField] private int _damageAmount = 10;
    [SerializeField] private float _replaceTime = 3f;
    [SerializeField] private Vector2 _targetPosition;
    private Vector2 _initialPosition;

    [Header("Light Shatter SoundEffect")]
    [SerializeField] private AudioClip _lightShatter;

    public int MaxHealth { get; set; }
    public int CurrentHealth { get; set; }
    public int DamageAmount { get; set; }

    private void OnEnable()
    {
        //Assign health 
        MaxHealth = _maxHealth;
        DamageAmount = _damageAmount;

        CurrentHealth = MaxHealth;
    }
   

    private void Start()
    {
        //Get the light Collider
        _lightCollider = _lightBulb.GetComponent<BoxCollider2D>();

        //Set the initial position
        _initialPosition = _lightPoll.transform.position;
        _targetPosition = new Vector2(_lightPoll.transform.position.x, _lightPoll.transform.position.y - 3f);
    }

    private IEnumerator ReplaceLight()
    {
        //Move Down
        yield return StartCoroutine(MoveTo(_targetPosition));

        _lightBulb.SetActive(true);
        _light.SetActive(false);
        ResetLightHealth();

        yield return new WaitForSeconds(_replaceTime);

        //Move back up
        yield return StartCoroutine(MoveTo(_initialPosition));
        _light.SetActive(true);
        
    }

    public void RecieveHit(RaycastHit2D RayHit, Vector2 hitDirection)
    {
        if (CurrentHealth <= 0) return;

        CurrentHealth -= DamageAmount;

        
        if(CurrentHealth <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        //Spawn Light shatter effect
        PoolManager.SpawnObject(_lightImpactCollision, transform.position, Quaternion.identity, PoolManager.PoolType.ParticleSystem);

        SFXManager._instance.playSFX(_lightShatter, _lightBulb.transform.position, 1f, true, false);

        _lightBulb.SetActive(false);

        StartCoroutine(ReplaceLight());
    }

    private IEnumerator MoveTo(Vector2 target)
    {
        float speed = 2f;

        while(Vector2.Distance(_lightPoll.transform.position, target) > 0.01f)
        {
            _lightPoll.transform.position = Vector2.Lerp(_lightPoll.transform.position, target, speed * Time.deltaTime);

            yield return null;
        }

        _lightPoll.transform.position = target;
    }

    private void ResetLightHealth()
    {
        CurrentHealth = MaxHealth;
    }
}
