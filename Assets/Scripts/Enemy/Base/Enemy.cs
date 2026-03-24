using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

//This is the base class for all the Enemy, and this inherits from two interfaces and all the stats and configs will be modified here

public class Enemy : MonoBehaviour, IDamageable, IEnemyMovable, IUpdateObserver, IFixedUpdateObserver, ILateUpdateObserver
{

    [field: SerializeField] public int MaxHealth { get; set; }
    [field: SerializeField] public int CurrentHealth { get; set; }
    [field: SerializeField] public int DamageAmount { get; set; }

    public Rigidbody2D RB { get; set; }
    private EnemyFlashEffect _flashEffect;
    public EnemyDissolveEffect _dissolveEffect;
    private HealthBar _healthBar;
    private TextMeshPro _healthText;
    public Animator _animator;
    private Vector3 _startingPos;

    [Header("Enemy SFX")]
    public AudioSource _audioSource;

    [Header("Enemy Visuals")]
    public GameObject _enemyVisuals;
    public SpriteRenderer _enemyVisualSpriteRenderer;
    public Material _enemyVisualMaterial;

    [Header("Camera")]
    private Transform _cameraTransform;

    public bool isFacingRight { get; set; } = true;

    [Header("SFX")]
    [SerializeField] private AudioClip _hurtSoundEffect;
    [SerializeField] private AudioClip _deathSoundEffect;

    #region State Machine Variables

    // We are using this variable to grab the instance of these classes they are not monobehaviour so the instance isnt created automatically
    public EnemyStateMachine stateMachine { get; set; }
    public EnemyState IdleState { get; set; }
    public EnemyState chaseState { get; set; }
    public EnemyState attackState { get; set; }

    #endregion

    public EnemyType _enemyType;

    private void OnEnable()
    {
        EnemyOnEnable();
    }

    
    public virtual void EnemyOnEnable()
    {

        UpdateManager.RegisterObserver(this);
        FixedUpdateManager.RegisterObserver(this);
        LateUpdateManager.RegisterObserver(this);

        AssignHealthAttributes();

        //Get the SpriteRenderer From the EnemyVisual
        _enemyVisualSpriteRenderer = _enemyVisuals.GetComponent<SpriteRenderer>();
        _enemyVisualMaterial = _enemyVisualSpriteRenderer.material;

        _dissolveEffect = _enemyVisuals.GetComponent<EnemyDissolveEffect>();


        if (_healthBar != null)
        {
            _healthBar.UpdateHealthBar(MaxHealth, CurrentHealth);
        }
        
        if (_flashEffect != null)
        {
            _flashEffect.ResetFlash();
        }


        if (_animator != null)
        {
            _animator.Rebind(); // reset statemachine & parameters
            _animator.Update(0f); //apply immediatly
        }

        if (stateMachine != null && IdleState != null)
        {
            //Debug.Log("Initializing state machine...");
            stateMachine.initialize(IdleState);
        }
        else
        {
            Debug.LogError("StateMachine or idleState is null in Awake");
        }

        //im justing doing this manually to stop the audio for now
        _audioSource.Stop();
    }

    private void Awake()
    {
        //Here we are setting up the instances for those classes
        stateMachine = new EnemyStateMachine();

        _healthText = GetComponentInChildren<TextMeshPro>();

        RB = GetComponent<Rigidbody2D>();

        switch (_enemyType)
        {
            case EnemyType.Chaser:
                IdleState = new Chaser_Idle_State(this, stateMachine);
                chaseState = new Chaser_Chase_State(this, stateMachine);
                attackState = null;
                break;
            case EnemyType.Tracer:
                IdleState = new Tracer_Idle_State(this, stateMachine);
                chaseState = new Tracer_Retreat_State(this, stateMachine);
                attackState = new Tracer_Attack_State(this, stateMachine);
                break;
            case EnemyType.Smasher:
                IdleState = new Smasher_Idle_State(this, stateMachine);
                chaseState = new Smasher_Chase_State(this, stateMachine);
                attackState = new Smasher_Attack_State(this, stateMachine);
                break;
        }
    }

    public void AssignHealthAttributes()
    {
        GameObject enemyPrefab = GameManager._instance.GetPrefabByEnemyType(_enemyType);

        if(GameManager._instance != null && GameManager._instance.TryGetEnemyData(enemyPrefab, out var data))
        //Setting the health and the DamageDeal Amount
        switch (_enemyType)
        {
            case EnemyType.Chaser:
                MaxHealth = data._health;
                DamageAmount = data._damageDealAmout;
                break;

            case EnemyType.Tracer:
                MaxHealth = data._health;
                DamageAmount = data._damageDealAmout;
                break;

            case EnemyType.Smasher:
                MaxHealth = data._health;
                DamageAmount = data._damageDealAmout;
                break;
        }

        // Setting the current health to MaxHealth
        CurrentHealth = MaxHealth;

        //Lets Reset the health text here 
        if (_healthText != null)
        {
            ResetHealthText(MaxHealth);
        }
    }

    public virtual void EnemyOnStart() 
    {
        //RB = GetComponent<Rigidbody2D>();
        _healthBar = GetComponentInChildren<HealthBar>();

        _animator = _enemyVisuals.GetComponent<Animator>();
        _flashEffect = _enemyVisuals.GetComponent<EnemyFlashEffect>();

        //Enemy Starting Position
        _startingPos = _enemyVisuals.transform.localScale;
    }

    private void Start()
    {
        EnemyOnStart();
    }

    public void ObservedUpdate()
    {
        if(stateMachine.currentEnemyState != null)
            stateMachine.currentEnemyState.FrameUpdate();

        UpdateEnemyAudioSource();
    }

    public void ObservedFixedUpdate()
    {
        if (stateMachine.currentEnemyState != null)
            stateMachine.currentEnemyState.PhysicsUpdate();
    }

    public void ObservedLateUpdate()
    {
        if(stateMachine.currentEnemyState != null)
            stateMachine.currentEnemyState.LateFrameUpdate();
    }

    #region Health / Die
    public void RecieveHit(RaycastHit2D RayHit, Vector2 hitDirection)
    {
        if (!gameObject.activeInHierarchy && CurrentHealth <= 0) return;
        
        CurrentHealth -= DamageAmount;

        if (_flashEffect != null)
        {
           
            _flashEffect.CallDamageFlash();
        }
        else
        {
            Debug.LogError($"[{_enemyType}] NO FLASH EFFECT! _flashAndDissolveEffect is NULL");
        }

        //Hurt SoundEffect
        if(_hurtSoundEffect != null)
        {
            SFXManager._instance.playSFX(_hurtSoundEffect, transform.position, 1f, true, false);
        }


        //Spawn Blood Effect
        BloodFXController.instance.PlayBloodFX(RayHit.point, RayHit.normal, hitDirection, BloodStainSpawnManager.CharacterType.Enemy);

        _healthBar.UpdateHealthBar(MaxHealth, CurrentHealth);
        UpdateHealthText(CurrentHealth);

        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    private void UpdateHealthText(int currentHealth)
    {
        if(_healthText != null)
        {
            _healthText.text = currentHealth.ToString();
        }
    }

    private void ResetHealthText(int maxHealth)
    {
        if (_healthText != null)
        {
            _healthText.text = maxHealth.ToString();
        }
    }

    public void Die()
    {
        GameManager._instance.AddScoreForEnemy(_enemyType);

        //Death Sound Effect
        if(_deathSoundEffect != null)
        {
            SFXManager._instance.playSFX(_deathSoundEffect, transform.position, 1f, true, false);
        }

        //Notify Game Manager before pooling/destroying
        GameManager._instance.EnemyDestroyed(gameObject);

        //We pool the enemy so that we can reuse it
        PoolManager.ReturnObjectToPool(gameObject, PoolManager.PoolType.GameObjects);
    }
    #endregion

    #region Movement Functions And Flipping 
    public virtual void MoveEnemy(Vector2 velocity)
    {
        RB.velocity = velocity;
        CheckForLeftorRightFacing(velocity);
    }

    public void CheckForLeftorRightFacing(Vector2 direction)
    {
        if(direction.x > 0 && !isFacingRight)
        {
            isFacingRight = true;
        }
        else if(direction.x < 0 && isFacingRight)
        {
           isFacingRight= false;
        }

        Flip();
    }

    private void Flip()
    {
        Vector3 targetScale = isFacingRight ? _startingPos : new Vector3(-_startingPos.x, _startingPos.y, _startingPos.z);
        _enemyVisuals.transform.localScale = targetScale;
    }

    #endregion

    #region Update Enemy Audio every Frame for the spatial Effect

    bool isInsideCameraView()
    {
        Vector3 viewPortPos = Camera.main.WorldToViewportPoint(transform.position);

        return viewPortPos.x >= 0 && viewPortPos.x <= 1 && viewPortPos.y >= 0 && viewPortPos.y <= 1;
    }

    private void UpdateEnemyAudioSource()
    {
        if(_cameraTransform == null)
        {
            //getting the camera transform
            _cameraTransform = Camera.main.transform;
        }

        if (_audioSource == null || !_audioSource.isPlaying)
        { return; }

        float dx = transform.position.x - _cameraTransform.position.x;
        float distance = Mathf.Abs(dx);

        //Volume based on distance
        float maxDistance = 10f;

        float volume = 1f - Mathf.Clamp01(distance / maxDistance);

        if (isInsideCameraView())
        {
            volume = Mathf.Max(volume, 0.4f);
            _audioSource.panStereo = 0f;
        }


        volume = Mathf.SmoothStep(0f, 1f, volume);
        _audioSource.volume = volume;

        // left right panning
        float pan = Mathf.Clamp(dx / maxDistance, -1f, 1f);
        _audioSource.panStereo = pan;
    }

    #endregion

    #region Animation Trigger Event

    public void AnimationTriggerEvent(AnimationTriggerType triggerType)
    {
        stateMachine.currentEnemyState.AnimationTriggerEvent(triggerType);
    }

    public enum AnimationTriggerType
    {
        EnemyIdle,
        ReadyToRun,
        Run,
        Jump,
        InAir,
        Fall,
        Land,
        Attack,
        EnemyDamaged,
        EnemyDead
    }

    #endregion

    public bool IsPlayerActive()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null && player.activeInHierarchy;
    }

    public virtual void EnemyOnDisable() 
    {
        UpdateManager.UnregisterObserver(this);
        FixedUpdateManager.UnregisterObserver(this);
        LateUpdateManager.UnregisterObserver(this);
    }

    private void OnDisable()
    {
        EnemyOnDisable();
    }

    //private void OnGUI()
    //{
    //    GUI.Label(new Rect(500, 10, 300, 100), $"Current State: {stateMachine.currentEnemyState?.GetType().Name}");
    //}
}
