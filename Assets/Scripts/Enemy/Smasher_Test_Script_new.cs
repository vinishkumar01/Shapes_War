using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UIElements;

public class Smasher_Test_Script_new : MonoBehaviour, IHittable
{
    [Header("Smasher Configs")]
    [SerializeField] int playerDetectionDistance;
    [SerializeField] float GroundCheckRadius;
    [SerializeField] float platformDetectionDistance;
    [SerializeField] float checkDistance;
    [SerializeField] float checkDistanceForJA;
    [SerializeField] float moveSpeed;
    [SerializeField] int facingDirection;
    [SerializeField] Vector2 rayDirection;

    [Header("Reference")]
    [SerializeField] GameObject GroundCheck;
    [SerializeField] GameObject platformSBCheck;
    [SerializeField] GameObject playerCheck;
    [SerializeField] GameObject distanceToPlayerCheck;
    [SerializeField] GameObject distancetoPlayerCheck_Jp;
    private HealthBar _healthBar;
    [SerializeField] private FlashEffect _flashEffect;

    [SerializeField] LayerMask platformLayer;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] Rigidbody2D rb;

    [Header("Conditions")]
    [SerializeField] bool isGrounded;
    [SerializeField] bool platformBelow;
    [SerializeField] bool platformside;
    [SerializeField] bool isPlayerDetected;
    [SerializeField] bool isplayerDetectedBack;
    [SerializeField] bool isPlayerNearToPorformSlam;
    [SerializeField] bool isPlayerNearToPerformJumpAttack;
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

    [Header("Animation")]
    [SerializeField] Animator NPCanimator;

    [Header("Health")]
    [SerializeField] int NPCMaxHealth = 200;
    [SerializeField] int NPCCurrenthealth;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        NPCanimator = GetComponent<Animator>();
        _flashEffect = GetComponent<FlashEffect>();
        _healthBar = GetComponentInChildren<HealthBar>();
        facingDirection = 1;
        
        //Rotation Config
        initialRotation = transform.rotation;
        targetRotation = Quaternion.Euler(0,0, slamAngle);

        spriteRenderer = GetComponent<SpriteRenderer>();

        //initialize the Health
        NPCCurrenthealth = NPCMaxHealth;
    }

    void IHittable.RecieveHit(RaycastHit2D RayHit)
    {
        Debug.Log("Got Hit: by Smasher");

        NPCCurrenthealth -= 20;

        _flashEffect.CallDamageFlash();

        //Update Health Bar
        _healthBar.UpdateHealthBar(NPCMaxHealth, NPCCurrenthealth);

        if (NPCCurrenthealth == 0)
        {
            Destroy(gameObject);
        }
    }


    // Update is called once per frame
    void Update()
    {
        DrawRaysAndSpheres();

        if(!isBusy)
        {
            if(isPlayerNearToPorformSlam)
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
            else if(isPlayerNearToPerformJumpAttack)
            {
                StartCoroutine(JumpAttack());
            }
            else
            {
                MoveAndChase();
                Flip();
                flipWhenPlayerDetected();
            }
        }

    }
    
    void DrawRaysAndSpheres()
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
        if(!isPlayerNearToPerformJumpAttack && !isJumping)
        {
            isPlayerNearToPorformSlam = Physics2D.Raycast(distanceToPlayerCheck.transform.position, rayDirection, checkDistance, playerLayer);

            Debug.DrawLine(distanceToPlayerCheck.transform.position, distanceToPlayerCheck.transform.position + (Vector3)(rayDirection * checkDistance), isPlayerNearToPorformSlam ? Color.red : Color.black);
        }
        if(!isPlayerNearToPorformSlam && !isSlaming)
        {
            isPlayerNearToPerformJumpAttack = Physics2D.Raycast(distancetoPlayerCheck_Jp.transform.position, rayDirection, checkDistanceForJA, playerLayer);

            Debug.DrawLine(distancetoPlayerCheck_Jp.transform.position, distancetoPlayerCheck_Jp.transform.position + (Vector3)(rayDirection * checkDistanceForJA), isPlayerNearToPerformJumpAttack ? Color.green : Color.white);
        }
    }

    void MoveAndChase()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(facingDirection * Time.deltaTime * moveSpeed, rb.velocity.y);
        }

        if (isPlayerNearToPorformSlam && !isJumping)
        {
            rb.velocity = new Vector2(0, 0);
        }
    }

    void Flip()
    {
        if (!platformBelow || platformside)
        {
            facingDirection *= -1;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    void flipWhenPlayerDetected()
    {
        if (isPlayerDetected && facingDirection == -1)
        {
            facingDirection = 1; // face right logically
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        if (isplayerDetectedBack && facingDirection == 1)
        {
            facingDirection = -1; // face left logically    
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    IEnumerator JumpAttack()
    {
        isBusy = true;
        isJumping = true;

        rb.gravityScale = acentGravity;
        rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);

        yield return new WaitForSeconds(SecondsToReachApex);

        while (rb.velocity.y > 0) // wailt until vertical velocity is downward or zero
            yield return null;

        rb.gravityScale = descentGravity;

        while(!isGrounded)
            yield return null;

        //Animation Triggered here
        NPCanimator.SetTrigger("Jump_Attack");

        rb.gravityScale = acentGravity;

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
        if(slamAngle > 0)
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

        while(rotatedAngle < Mathf.Abs(slamAngle))
        {
            //Calculate rotation increment this frame
            float step = SlamSpeed * Time.deltaTime;

            if (rotatedAngle + step > Mathf.Abs(slamAngle))
                step = Mathf.Abs(slamAngle) - rotatedAngle;

            //Rotate around bottom right pivot point on Z axis
            transform.RotateAround(bottomPivot, Vector3.forward, slamAngle > 0 ? step : -step);
            NPCanimator.SetTrigger("Slam_Attack");
            rotatedAngle += step;

            yield return null;
        }

        //pause briefly at the bottom
        yield return new WaitForSeconds(pauseBeforeReturn);

        //Reset rotateAngle for return rotation
        rotatedAngle = 0f;

        //Rotate Backup
        while(rotatedAngle < Mathf.Abs(slamAngle))
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
        if(isGrounded)
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
        if(platformside)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(platformSBCheck.transform.position, platformSBCheck.transform.position + (Vector3)(rayDirection * platformDetectionDistance));
        }
        else
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(platformSBCheck.transform.position, platformSBCheck.transform.position + (Vector3)(rayDirection * platformDetectionDistance));
        }

        if(platformBelow)
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
            damageable.Damage(25, hitDirection);
        }
    }
}
