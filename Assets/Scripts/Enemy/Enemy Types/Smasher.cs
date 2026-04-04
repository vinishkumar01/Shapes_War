using System.Collections;
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
    [SerializeField] GameObject platformSBCheckHead;
    [SerializeField] GameObject playerCheck;
    [SerializeField] GameObject distanceToPlayerCheck;
    [SerializeField] GameObject distancetoPlayerCheck_Jp;
    [SerializeField] private ParticleSystem Dust;

    [SerializeField] LayerMask platformLayer;
    [SerializeField] LayerMask playerLayer;

    [Header("Conditions")]
    public bool isGrounded;
    [SerializeField] bool platformBelow;
    [SerializeField] bool platformBelowToSlam;
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

    [Header("Smasher Visuals")]
    private SquashAndStretch _smasherSquashAndStretch;

    [Header("SFX")]
    [SerializeField] private AudioClip _smasherJumpEffect;
    [SerializeField] private AudioClip _smasherLandEffect;
    [SerializeField] private AudioClip _smasherSlamEffect;

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

        spriteRenderer = _enemyVisuals.GetComponent<SpriteRenderer>();

        //Visual
        _smasherSquashAndStretch = _enemyVisuals.GetComponent<SquashAndStretch>();

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
        //Checking Platform walls this is located in the head of the smasher and also this will be used when slam attack to check that it has collided with the Ground, So basically to functionality
        platformside = Physics2D.Raycast(platformSBCheckHead.transform.position, rayDirection, platformDetectionDistance, platformLayer);
        platformBelowToSlam = Physics2D.Raycast(platformSBCheckHead.transform.position, Vector2.down, platformDetectionDistance, platformLayer);

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

            //Play the dust particle when the smasher moves (as its a heavy character its expected to see some dust effect when it moves)
            if(Mathf.Abs(RB.velocity.x) > 1)
            {
                Dust.Play();
            }
            else
            {
                Dust.Stop();
            }

            //Sound effect for movement
            if(GameState.CanPlayerControl)
            {
                if (!_audioSource.isPlaying)
                {
                    _audioSource.Play();
                }
            }
        }
        else
        {
            Dust.Stop();

            if (_audioSource.isPlaying)
            {
                _audioSource.Stop();
            }
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

    private void FlipPlatformCheckPositionManually()
    {
        //Flipping the platformSBCheck manually as we are flipping the visuals only in the base class (Enemy.cs) 
        Vector2 SBCheckscale = platformSBCheck.transform.localPosition;
        SBCheckscale.x = Mathf.Abs(SBCheckscale.x) * facingDirection;
        platformSBCheck.transform.localPosition = SBCheckscale;

        //platformCheck from Head
        Vector2 SBCheckHeadscale = platformSBCheckHead.transform.localPosition;
        SBCheckHeadscale.x = Mathf.Abs(SBCheckHeadscale.x) * facingDirection;
        platformSBCheckHead.transform.localPosition = SBCheckHeadscale;
    }


    public void FlipToAvoidEdges()
    {
        if ((isBusy || isJumping || isSlaming)) return;

        if (!platformBelow || platformside)
        {
            facingDirection = -facingDirection;

            FlipPlatformCheckPositionManually();

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

            FlipPlatformCheckPositionManually();

            Vector2 velocity = RB.velocity;
            velocity.x = Mathf.Abs(velocity.x);
            MoveEnemy(new Vector2(velocity.x, velocity.y));
        }

        if (isplayerDetectedBack && facingDirection == 1)
        {
            facingDirection = -1;

            FlipPlatformCheckPositionManually();

            Vector2 velocity = RB.velocity;
            velocity.x = -Mathf.Abs(velocity.x);
            MoveEnemy(new Vector2(velocity.x, velocity.y));
        }
    }

    IEnumerator JumpAttack()
    {
        isBusy = true;
        isJumping = true;

        //Applying Jump Stretch
        if (facingDirection > 0)
        {
            _smasherSquashAndStretch.Squash(-0.08f, 0.06f);
        }
        else
        {
            _smasherSquashAndStretch.Squash(0.08f, 0.06f);
        }

        RB.gravityScale = acentGravity;
        RB.velocity = new Vector2(RB.velocity.x, jumpVelocity);

        //Play the dust particle when the smasher jumps
        Dust.Play();

        //Sound Effect for jump
        SFXManager._instance.playSFX(_smasherJumpEffect, gameObject.transform.position, 1f, false);

        yield return new WaitForSeconds(SecondsToReachApex);

        while (RB.velocity.y > 0) // wailt until vertical velocity is downward or zero
            yield return null;

        RB.gravityScale = descentGravity;

        while (!isGrounded)
            yield return null;

        //Applying Landing Squash
        if (facingDirection > 0)
        {
            _smasherSquashAndStretch.Squash(0.18f, -0.22f);
        }
        else
        {
            _smasherSquashAndStretch.Squash(-0.18f, -0.22f);
        }

        //Play the dust particle when the smasher lands
        Dust.Play();

        //Animation Triggered here
        _animator.SetTrigger("Jump_Attack");

        //Sound Effect for landing
        SFXManager._instance.playSFX(_smasherLandEffect, gameObject.transform.position, 1f, true, false);

        RB.gravityScale = acentGravity;

        yield return new WaitForSeconds(pauseBeforeNextJump);

        isJumping = false;
        isBusy = false;
    }

    IEnumerator Slam()
    {
        isBusy = true;
        isSlaming = true;

        //Stop the dust particle when the smasher slams
        Dust.Stop();

        _animator.ResetTrigger("Slam_Attack");

        //Slam Sound Effect
        SFXManager._instance.playSFX(_smasherSlamEffect, gameObject.transform.position, 1f, true , false);
        

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
            
            if(platformBelowToSlam)
            {
                _animator.SetTrigger("Slam_Attack");
            }

            rotatedAngle += step;

            yield return null;
        }

        //Applying Squash and Stretch
        if (facingDirection > 0 && platformBelowToSlam)
        {
            _smasherSquashAndStretch.Squash(
               0.16f,   // widen
              -0.20f   // compress
           );
        }
        else if (facingDirection < 0 && platformBelowToSlam)
        {
            _smasherSquashAndStretch.Squash(
               -0.16f,   // widen
              -0.20f   // compress
           );
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
            Gizmos.DrawLine(platformSBCheckHead.transform.position, platformSBCheckHead.transform.position + (Vector3)(rayDirection * platformDetectionDistance));
        }
        else
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(platformSBCheck.transform.position, platformSBCheck.transform.position + (Vector3)(rayDirection * platformDetectionDistance));
            Gizmos.DrawLine(platformSBCheckHead.transform.position, platformSBCheckHead.transform.position + (Vector3)(rayDirection * platformDetectionDistance));
        }

        if (platformBelow)
        {
            Gizmos.color = Color.red;
            Debug.DrawLine(platformSBCheck.transform.position, (Vector2)platformSBCheck.transform.position + Vector2.down * platformDetectionDistance);
            Debug.DrawLine(platformSBCheckHead.transform.position, (Vector2)platformSBCheckHead.transform.position + Vector2.down * platformDetectionDistance);

        }
        else
        {
            Gizmos.color = Color.white;
            Debug.DrawLine(platformSBCheck.transform.position, (Vector2)platformSBCheck.transform.position + Vector2.down * platformDetectionDistance);
            Debug.DrawLine(platformSBCheckHead.transform.position, (Vector2)platformSBCheckHead.transform.position + Vector2.down * platformDetectionDistance);
        }

        if (platformBelowToSlam)
        {
            Gizmos.color = Color.red;
            Debug.DrawLine(platformSBCheckHead.transform.position, (Vector2)platformSBCheckHead.transform.position + Vector2.down * platformDetectionDistance);

        }
        else
        {
            Gizmos.color = Color.white;
            Debug.DrawLine(platformSBCheckHead.transform.position, (Vector2)platformSBCheckHead.transform.position + Vector2.down * platformDetectionDistance);
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out IPlayerDamageable damageable))
        {
            ContactPoint2D contact = collision.GetContact(0);
            Vector2 hitPoint = contact.point;
            Vector2 hitNormal = contact.normal;

            Vector2 hitDirection = (collision.transform.position - transform.position).normalized;
            damageable.Damage(_smasherDamageGives, hitDirection, hitPoint, hitNormal);
        }
    }
}
