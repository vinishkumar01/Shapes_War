using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;

public class Smasher : Enemy
{
    [Header("Smasher Configs")]
    [SerializeField] float GroundCheckRadius;
    [SerializeField] float platformDetectionDistance;
    [SerializeField] float checkDistance;
    [SerializeField] float checkDistanceForJA;
    int moveSpeed { get; set; }
    int playerDetectionDistance { get; set; }
    [SerializeField] int facingDirection;
    [SerializeField] Vector2 rayDirection;

    [Header("Reference")]
    [SerializeField] GameObject GroundCheck;
    [SerializeField] GameObject platformSBCheck;
    [SerializeField] GameObject playerCheck;
    [SerializeField] GameObject distanceToPlayerCheck;
    [SerializeField] GameObject distancetoPlayerCheck_Jp;

    [SerializeField] LayerMask platformLayer;
    [SerializeField] LayerMask playerLayer;

    [Header("Conditions")]
    [SerializeField] bool isGrounded;
    [SerializeField] bool platformBelow;
    [SerializeField] bool platformside;
    [SerializeField] bool isPlayerDetected;
    [SerializeField] bool isplayerDetectedBack;
    [SerializeField] public bool isPlayerNearToPorformSlam;
    [SerializeField] public bool isPlayerNearToPerformJumpAttack;
    [SerializeField] bool flipactive;

    [Header("Attack Config")]
    //Slam Config
    [SerializeField] float slamAngle = 90f;
    [SerializeField] float SlamSpeed = 100f;
    [SerializeField] float returnSpeed = 150f;
    [SerializeField] float pauseBeforeReturn = 0.5f;
    [SerializeField] float coolDown = 2f;

    [SerializeField] bool isSlaming = false;
    Quaternion initialRotation;
    Quaternion targetRotation;
    Vector3 bottomPivot;
    SpriteRenderer spriteRenderer;

    //Jump Attack
    [SerializeField] float jumpVelocity = 40f;
    [SerializeField] float acentGravity = 5f;
    [SerializeField] float descentGravity = 25f;
    [SerializeField] float SecondsToReachApex = 0.1f;
    [SerializeField] float pauseBeforeNextJump = 1f;

    [SerializeField] bool isJumping;
    [SerializeField] bool isBusy;

    private int _smasherDamageGives { get; set; }

    public override void EnemyOnEnable()
    {
        base.EnemyOnEnable();

        AssignSmasherAttributes();
        ResetSmasherState();
    }

    public override void EnemyOnDisable()
    {
        base.EnemyOnDisable();

        StopAllCoroutines();
    }

    public override void EnemyOnStart()
    {
        base.EnemyOnStart();

        spriteRenderer = GetComponent<SpriteRenderer>();

        facingDirection = 1;

        //Rotation Config
        initialRotation = transform.rotation;
        targetRotation = Quaternion.Euler(0, 0, slamAngle);
    }

    private void ResetSmasherState()
    {
        isSlaming = false;
        isplayerDetectedBack = false;
        isPlayerDetected = false;   
        isBusy = false;
    }

    private void AssignSmasherAttributes()
    {
        GameObject smasherPrefab = GameManager._instance.GetPrefabByEnemyType(EnemyType.Smasher);

        if (GameManager._instance != null && GameManager._instance.TryGetEnemyData(smasherPrefab, out var data))
        {
            _smasherDamageGives = data._damageGives;

            moveSpeed = data._moveSpeed;
            playerDetectionDistance = data._playerDetectionDistance;
            Debug.Log($"Assigning the data from GameManager: Smasher Damage Gives{_smasherDamageGives}");
        }
        else
        {
            Debug.LogError("No GameManager data for Smasher");
            _smasherDamageGives = 30;
            moveSpeed = 600;
            playerDetectionDistance =  10;
        }
    }

    //Cast Rays
    public void DrawRaysAndSpheres()
    {
        rayDirection = Vector2.right * facingDirection;

        //Grounded 
        isGrounded = Physics2D.OverlapCircle(GroundCheck.transform.position, GroundCheckRadius, platformLayer);

        //PLtform Check
        platformBelow = Physics2D.Raycast(platformSBCheck.transform.position, Vector2.down, platformDetectionDistance, platformLayer);
        platformside = Physics2D.Raycast(platformSBCheck.transform.position, rayDirection, platformDetectionDistance, platformLayer);

        //PlayerCheck
        isPlayerDetected = Physics2D.Raycast(playerCheck.transform.position, transform.right, playerDetectionDistance, playerLayer);
        isplayerDetectedBack = Physics2D.Raycast(playerCheck.transform.position, -transform.right, playerDetectionDistance, playerLayer);


        Debug.DrawLine(playerCheck.transform.position, playerCheck.transform.position + transform.right * playerDetectionDistance, isPlayerDetected ? Color.red : Color.yellow);
        Debug.DrawLine(playerCheck.transform.position, playerCheck.transform.position + -transform.right * playerDetectionDistance, isplayerDetectedBack ? Color.red : Color.magenta);

        //Stop at certain distance when moving towards to the player
        if (!isPlayerNearToPerformJumpAttack && !isJumping)
        {
            isPlayerNearToPorformSlam = Physics2D.Raycast(distanceToPlayerCheck.transform.position, rayDirection, checkDistance, playerLayer);

            Debug.DrawLine(distanceToPlayerCheck.transform.position, distanceToPlayerCheck.transform.position + (Vector3)(rayDirection * checkDistance), isPlayerNearToPorformSlam ? Color.red : Color.black);
        }
        if (!isPlayerNearToPorformSlam && !isSlaming)
        {
            isPlayerNearToPerformJumpAttack = Physics2D.Raycast(distancetoPlayerCheck_Jp.transform.position, rayDirection, checkDistanceForJA, playerLayer);

            Debug.DrawLine(distancetoPlayerCheck_Jp.transform.position, distancetoPlayerCheck_Jp.transform.position + (Vector3)(rayDirection * checkDistanceForJA), isPlayerNearToPerformJumpAttack ? Color.green : Color.white);
        }
    }

    public void Move()
    {
        if (isGrounded)
        {
            Vector2 velocity = new Vector2(facingDirection * Time.deltaTime * moveSpeed, RB.velocity.y);
            MoveEnemy(velocity);
        }

        if (isPlayerNearToPorformSlam && !isJumping)
        {
            MoveEnemy(Vector2.zero);
        }
    }

    public void MoveAndAttack()
    {
        if (!isBusy)
        {
            if (isPlayerNearToPorformSlam)
            {
                if (isplayerDetectedBack)
                {
                    slamAngle = 90f;
                }
                else if (isPlayerDetected)
                {
                    slamAngle = -90f;
                }
                StartCoroutine(Slam());
            }
            else if (isPlayerNearToPerformJumpAttack)
            {
                StartCoroutine(JumpAttack());
            }
            else
            {
                Move();
                FlipToAvoidEdges();
                FlipWhenPlayerDetected();
            }
        }
    }

    public void FlipToAvoidEdges()
    {
        if ((isBusy || isJumping || isSlaming)) return;

        if (!platformBelow || platformside)
        {
            facingDirection = -facingDirection;
            Vector2 velocity = RB.velocity;
            velocity.x = facingDirection * Mathf.Abs(moveSpeed * Time.deltaTime);
            MoveEnemy(new Vector2(velocity.x, velocity.y));
        }
    }

   public void FlipWhenPlayerDetected()
    {
        if ((isBusy || isJumping || isSlaming)) return;

        if (isPlayerDetected && facingDirection == -1)
        {
            facingDirection = 1;
            Vector2 velocity = RB.velocity;
            velocity.x = Mathf.Abs(velocity.x);
            MoveEnemy(new Vector2(velocity.x, velocity.y));
        }

        if (isplayerDetectedBack && facingDirection == 1)
        {
            facingDirection = -1;
            Vector2 velocity = RB.velocity;
            velocity.x = -Mathf.Abs(velocity.x);
            MoveEnemy(new Vector2(velocity.x, velocity.y));
        }
    }

    IEnumerator JumpAttack()
    {
        isBusy = true;
        isJumping = true;

        RB.gravityScale = acentGravity;
        RB.velocity = new Vector2(RB.velocity.x, jumpVelocity);

        yield return new WaitForSeconds(SecondsToReachApex);

        while (RB.velocity.y > 0) // wailt until vertical velocity is downward or zero
            yield return null;

        RB.gravityScale = descentGravity;

        while (!isGrounded)
            yield return null;

        //Animation Triggered here
        _animator.SetTrigger("Jump_Attack");

        RB.gravityScale = acentGravity;

        yield return new WaitForSeconds(pauseBeforeNextJump);

        isJumping = false;
        isBusy = false;
    }

    IEnumerator Slam()
    {
        isBusy = true;
        isSlaming = true;

        //Calculate bottom right pivot from spriteRenderer bounds
        Bounds bounds = spriteRenderer.bounds; // world space bounds

        //decide pivot based on slam direction
        if (slamAngle > 0)
        {
            //Left Slam
            bottomPivot = new Vector3(bounds.min.x, bounds.min.y, transform.position.z);
        }
        else
        {
            // Right Slam
            bottomPivot = new Vector3(bounds.max.x, bounds.min.y, transform.position.z);
        }

        //Angle rotated so far
        float rotatedAngle = 0f;

        while (rotatedAngle < Mathf.Abs(slamAngle))
        {
            //Calculate rotation increment this frame
            float step = SlamSpeed * Time.deltaTime;

            if (rotatedAngle + step > Mathf.Abs(slamAngle))
                step = Mathf.Abs(slamAngle) - rotatedAngle;

            //Rotate around bottom right pivot point on Z axis
            transform.RotateAround(bottomPivot, Vector3.forward, slamAngle > 0 ? step : -step);
            _animator.SetTrigger("Slam_Attack");
            rotatedAngle += step;

            yield return null;
        }

        //pause briefly at the bottom
        yield return new WaitForSeconds(pauseBeforeReturn);

        //Reset rotateAngle for return rotation
        rotatedAngle = 0f;

        //Rotate Backup
        while (rotatedAngle < Mathf.Abs(slamAngle))
        {
            float step = returnSpeed * Time.deltaTime;

            if (rotatedAngle + step > Mathf.Abs(slamAngle))
                step = Mathf.Abs(slamAngle) - rotatedAngle;

            //Rotate Backwards around the pivot
            transform.RotateAround(bottomPivot, Vector3.forward, slamAngle > 0 ? -step : step);
            rotatedAngle += step;

            yield return null;
        }

        //CoolDown before the next slam
        yield return new WaitForSeconds(coolDown);

        isSlaming = false;
        isBusy = false;
    }


    private void OnDrawGizmos()
    {
        //WireSphere
        if (isGrounded)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GroundCheck.transform.position, GroundCheckRadius);
        }
        else
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(GroundCheck.transform.position, GroundCheckRadius);
        }

        //Platform Check Line
        if (platformside)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(platformSBCheck.transform.position, platformSBCheck.transform.position + (Vector3)(rayDirection * platformDetectionDistance));
        }
        else
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(platformSBCheck.transform.position, platformSBCheck.transform.position + (Vector3)(rayDirection * platformDetectionDistance));
        }

        if (platformBelow)
        {
            Gizmos.color = Color.red;
            Debug.DrawLine(platformSBCheck.transform.position, (Vector2)platformSBCheck.transform.position + Vector2.down * platformDetectionDistance);
        }
        else
        {
            Gizmos.color = Color.white;
            Debug.DrawLine(platformSBCheck.transform.position, (Vector2)platformSBCheck.transform.position + Vector2.down * platformDetectionDistance);
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out IPlayerDamageable damageable))
        {
            Vector2 hitDirection = (collision.transform.position - transform.position).normalized;
            damageable.Damage(_smasherDamageGives, hitDirection);
        }
    }
}
