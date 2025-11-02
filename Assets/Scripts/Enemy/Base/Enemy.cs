using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is the base class for all the Enemy, and this inherits from two interfaces and all the stats and configs will be modified here

public class Enemy : MonoBehaviour, IDamageable, IEnemyMovable, ITriggerCheckable
{
    [field: SerializeField] public float MaxHealth { get; set; } = 100f;

    public float CurrentHealth { get; set; }

    public Rigidbody2D RB { get; set; }

    public bool isFacingRight { get; set; } = false;

    #region State Machine Variables

    // We are using this variable to grab the instance of these classes they are not monobehaviour so the instance isnt created automatically
    public EnemyStateMachine stateMachine { get; set; }

    public EnemyIdleState idleState { get; set; }

    public EnemyChaseState chaseState { get; set; }

    public EnemyAttackState attackState { get; set; }

    public bool isAggroed { get; set; }

    public bool IsWithinStrikingDistance { get; set; }

    #endregion

    #region Idle Variable

    public float RandomMovementRange = 3f;
    public float RandomMovementSpeed = 10f;

    #endregion

    private void Awake()
    {
        //Here we are setting up the instances for those classes
        stateMachine = new EnemyStateMachine();

        idleState = new EnemyIdleState(this, stateMachine);
        chaseState = new EnemyChaseState(this, stateMachine);
        attackState = new EnemyAttackState(this, stateMachine);
    }

    private void Start()
    {
        CurrentHealth = MaxHealth;

        RB = GetComponent<Rigidbody2D>();

        stateMachine.initialize(idleState);
    }

    private void Update()
    {
        stateMachine.currentEnemyState.FrameUpdate();
    }

    private void FixedUpdate()
    {
        stateMachine.currentEnemyState.PhysicsUpdate();
    }

    #region Health / Die
    public void Damage(float damageAmount)
    {
        CurrentHealth -= damageAmount;

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

    #region Movement Functions 
    public void MoveEnemy(Vector3 velocity)
    {
        RB.velocity = velocity;
        CheckForLeftorRightFacing(velocity);
    }

    public void CheckForLeftorRightFacing(Vector2 velocity)
    {
        if (isFacingRight && velocity.x < 0f)
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            isFacingRight = !isFacingRight;
        }
        else if(!isFacingRight && velocity.x > 0f) 
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            isFacingRight = !isFacingRight;
        }
    }

    #endregion

    #region Animation Trigger Event
    
    public void AnimationTriggerEvent(AnimationTriggerType triggerType)
    {
        stateMachine.currentEnemyState.AnimationTriggerEvent(triggerType);
    }

    public enum AnimationTriggerType
    {
        EnemyDamaged,
        PlayFootStepSound
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

    public void Damage(float damageAmount, Vector2 hitDirection)
    {
        throw new System.NotImplementedException();
    }

    #endregion
}
