using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is the base class for all the Enemy, and this inherits from two interfaces and all the stats and configs will be modified here

public class Enemy : MonoBehaviour, IDamageable, IEnemyMovable, ITriggerCheckable
{
    [SerializeField] protected EnemiesSO statsSO;

    [field: SerializeField] public int MaxHealth { get; set; }
    [field: SerializeField] public int CurrentHealth { get; set; }
    [field: SerializeField] public int DamageAmount { get; set; }

    public Rigidbody2D RB { get; set; }
    private FlashEffect _flashEffect;
    private HealthBar _healthBar;
    public Animator _animator;

    public bool isFacingRight { get; set; } = true;

    #region State Machine Variables

    // We are using this variable to grab the instance of these classes they are not monobehaviour so the instance isnt created automatically
    public EnemyStateMachine stateMachine { get; set; }
    public EnemyState IdleState { get; set; }
    public EnemyState chaseState { get; set; }
    public EnemyState attackState { get; set; }

    #endregion

    public bool isAggroed { get; set; }
    public bool IsWithinStrikingDistance { get; set; }


    public EnemyType _enemyType;

    private void Awake()
    {
        //Here we are setting up the instances for those classes
        stateMachine = new EnemyStateMachine();

        AssignHealthAttributes();

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
        //Setting the health and the DamageDeal Amount
        if(statsSO == null)
        {
            Debug.LogError($"statsSO is not assigned for enemy Type {_enemyType}");
        }

        switch (_enemyType)
        {
            case EnemyType.Chaser:
                MaxHealth = statsSO._chaserMaxHealth;
                DamageAmount = statsSO._chaserDamageDealAmount;
                break;

            case EnemyType.Tracer:
                MaxHealth = statsSO._tracerMaxHealth;
                DamageAmount = statsSO._tracerDamageDealAmount;
                break;

            case EnemyType.Smasher:
                MaxHealth = statsSO._smasherMaxHealth;
                DamageAmount = statsSO._smasherDamageDealAmount;
                break;
        }

        // Setting the current health to MaxHealth
        CurrentHealth = MaxHealth;
    }

    public virtual void EnemyOnStart() { }

    private void Start()
    {
        Debug.Log("Enemy Start() called");

        CurrentHealth = MaxHealth;

        RB = GetComponent<Rigidbody2D>();
        _flashEffect = GetComponent<FlashEffect>();
        _healthBar = GetComponentInChildren<HealthBar>();
        _animator = GetComponent<Animator>();

        EnemyOnStart();

        if (stateMachine != null && IdleState != null)
        {
            Debug.Log("Initializing state machine...");
            stateMachine.initialize(IdleState);
        }
        else
        {
            Debug.LogError("StateMachine or idleState is null in Awake");
        }

    }

    private void Update()
    {
        if(stateMachine.currentEnemyState != null)
            stateMachine.currentEnemyState.FrameUpdate();
    }

    private void FixedUpdate()
    {
        if (stateMachine.currentEnemyState != null)
            stateMachine.currentEnemyState.PhysicsUpdate();
    }

    private void LateUpdate()
    {
        if(stateMachine.currentEnemyState != null)
            stateMachine.currentEnemyState.LateFrameUpdate();
    }

    #region Health / Die
    public void RecieveHit(RaycastHit2D RayHit)
    {
        CurrentHealth -= DamageAmount;

        _flashEffect.CallDamageFlash();

        _healthBar.UpdateHealthBar(MaxHealth, CurrentHealth);

        if(CurrentHealth <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
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
            Flip();
        }
        else if(direction.x < 0 && isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
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

    #region Distance Checks

    public void SetAggroedStatus(bool aggroed)
    {
        isAggroed = aggroed;
    }

    public void SetStrikingDistance(bool isWithinStrikingDistance)
    {
        IsWithinStrikingDistance = isWithinStrikingDistance;
    }


    #endregion

    public bool IsPlayerActive()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null && player.activeInHierarchy;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(500, 10, 300, 100), $"Current State: {stateMachine.currentEnemyState?.GetType().Name}");
    }
}
