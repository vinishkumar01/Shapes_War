using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;
    [SerializeField] Animator playerAnimator;
    [SerializeField] TextMeshPro Dashtext;

    [Header("Gravity Configs")]
    [SerializeField]float gravityStrength; //Downwards force (gravity) needed for the desired jumpHeight and jumpTimeToApex.
    [SerializeField]public float gravityScale; //Strength of the player's gravity as a multiplier of gravity (set in ProjectSettings/Physics2D).
    [SerializeField] float fallGravityMult; //Multiplier to the player's gravityScale when falling.
    [SerializeField] float MaxFallSpeed; //Maximum fall speed (terminal velocity) of the player when falling.
    [SerializeField] float fastFallGravityMult; //Larger multiplier to the player's gravityScale when they are falling and a downwards input is pressed.
//Seen in games such as Celeste, lets the player fall extra fast if they wish.
    [SerializeField] float MaxFastFallSpeed; //Maximum fall speed(terminal velocity) of the player when performing a faster fall.  

    [Header("Jump Configs")]
    [SerializeField] float Jumpforce = 36.33333f;
    [SerializeField] float JumpHeight;
    [SerializeField] float JumpTimeToApex;
    [SerializeField] bool DoubleJump;
    [SerializeField] int MidAirJumpCount;
    [SerializeField] float DoubleJumpForce;

    [Header("Movement Configs")]
    [SerializeField]float MovementSpeed = 10.0f;
    [SerializeField]Vector2 MovementInputDirection;
    float VelocityXSmoothing;
    [SerializeField] float accelerationtimeAirborne = .2f;
    [SerializeField] float accelerationtimeGrounded = .1f;

    [Header("Surrounding Check")]
    [SerializeField]bool isGrounded;
    [SerializeField] float GroundCheckRadius;
    [SerializeField] Transform GroundCheck;
    [SerializeField] LayerMask Ground;

    [Header("Player Dash")]
    [SerializeField] bool IsDashing;
    [SerializeField] float dashTimeLeft;
    [SerializeField] float lastDash = -100f;
    [SerializeField] float DashTime = 0.2f;
    [SerializeField] float DashSpeed = 50f;
    [SerializeField] float DashCoolDown = 2.5f;

    [Header("Player Aiming & Flipping")]
    [SerializeField] Transform GunPivot;
    [SerializeField] Transform Gun;
    Vector3 StartingPos;
    


    [Header("Rope Configs")]
    [SerializeField] HingeJoint2D hingejoint;
    [SerializeField] float RopePushForce = 10f;
    [SerializeField] bool AttachedToRope = false;
    [SerializeField] Transform AttachedTo;
    GameObject disregard;
    [SerializeField] float reattachDelay = 0.5f;
    [SerializeField] float detachTime;
    [SerializeField] bool isRope;
    [SerializeField] float RopeCheckRadius;
    [SerializeField] Transform ropeCheck;
    [SerializeField] LayerMask Rope;
    [SerializeField] Rigidbody2D ropeInRange = null;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        
        //Getting the groundcheck gameObjects transform(position)
        GroundCheck = GameObject.Find("GC").GetComponent<Transform>();

        //HingeJoint
        hingejoint = gameObject.GetComponent<HingeJoint2D>();

        //Player Starting Position Configs
        StartingPos = transform.localScale;
        

        SetGravityScale(gravityScale);

        //Player Double Jump Config
        //amountOfJumpsLeft = amountOfJumps;
    }

    private void Update()
    {
        CheckInputs();
        CheckDash();
        CheckPlayerAttachedToRope();

    }

    private void FixedUpdate()
    {
        ApplyMovements();
        CheckSurrounding();
        RopeCheck();
        PlayerFlipping();
        GravityConfigs();

    }

    #region Checking player Inputs

    void CheckInputs()
    {
        MovementInputDirection.x = Input.GetAxisRaw("Horizontal");
        MovementInputDirection.y = Input.GetAxisRaw("Vertical");

        if(Input.GetKeyDown(KeyCode.Space))
        {
            PlayerJumps();

            if(AttachedToRope)
            {
                Detach();
            }
        }

        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            if(Time.time >= (lastDash + DashCoolDown))
            {
               AttemptToDash(); 
            }
        }
        
        //Rope Swing Inputs
        if(Input.GetKeyDown(KeyCode.A))
        {
            PlayerRopeJump(new Vector2(MovementInputDirection.x,0));
        }
        if(Input.GetKeyDown(KeyCode.D))
        {
            PlayerRopeJump(new Vector2(MovementInputDirection.x, 0));
        }
        
    }

    #endregion

    #region Player Gravity Configs

    void GravityConfigs()
    {
        // Calculating the gravity Strength using the below formula 
        //Physics2D.gravity.y is been set to -30 , Jump height = 3.5, jumptimetoapex = 0.3  
        gravityStrength = -(2 * JumpHeight) / (JumpTimeToApex * JumpTimeToApex);

        //calculate the rigidBody's gravity Scale(i.e. gravity strength relative to unity's gravity value)
        gravityScale = gravityStrength / Physics2D.gravity.y;

        // Calculate the jumpForce using the formula (InitialJumpVelocity = gravity * timetoJumpApex)
        Jumpforce = Mathf.Abs(gravityStrength) * JumpTimeToApex;
        DoubleJumpForce = Mathf.Abs(gravityStrength) * JumpTimeToApex;

        //
        if (rb.velocity.y < 0 && MovementInputDirection.y < 0)
        {
            // Much Higher gravity if Holding Down
            SetGravityScale(gravityScale * fastFallGravityMult);
            // Caps Maximum fall speed, so when falling over large distance we dont accelerate to insane fall speed 
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -MaxFastFallSpeed));
        }
        else if(rb.velocity.y < 0)
        {
            //Higher gravity if falling
            SetGravityScale(gravityScale * fallGravityMult);

            //Caps Maximum fall Speed, So when falling over larger distance we dont accelerate to insanly
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -MaxFallSpeed));
        }
        else
        {
            SetGravityScale(gravityScale);
        }
    }

    void SetGravityScale(float Scale)
    {
        rb.gravityScale = Scale;
    }

    #endregion

    #region Player Jumping Mechanisms
    void PlayerJumps()
    {
        if(isGrounded && !Input.GetKeyDown(KeyCode.Space))
        {
            DoubleJump = false;
            MidAirJumpCount = 1;
        }
        else if (!isGrounded && Input.GetKeyDown(KeyCode.Space) && MidAirJumpCount > 0)
        {
            DoubleJump = true;
            MidAirJumpCount = 0;
        }

        if (isGrounded || DoubleJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, DoubleJump ? DoubleJumpForce : Jumpforce);

            DoubleJump = !DoubleJump;
        }
        
    }

    void PlayerRopeJump(Vector2 MovementInputDirection)
    {
        if(AttachedToRope)
        {
            rb.AddRelativeForce(new Vector2(MovementInputDirection.x, 0) * RopePushForce);
        }
        
    }
    #endregion

    #region Player Movements
    void ApplyMovements()
    {
        float TargetVelocityX = MovementSpeed * MovementInputDirection.x;

        rb.velocity = new Vector2(Mathf.SmoothDamp(rb.velocity.x, TargetVelocityX, ref VelocityXSmoothing, (isGrounded)? accelerationtimeGrounded : accelerationtimeAirborne), rb.velocity.y);

        playerAnimator.SetInteger("isPlayerMoving", Mathf.Abs((int)TargetVelocityX));
    }

    #endregion

    #region Player Check Surrounding

    //Ground Check
    void CheckSurrounding()
    {
        isGrounded = Physics2D.OverlapCircle(GroundCheck.position, GroundCheckRadius,Ground); 
    }

    //Rope Check
    void RopeCheck()
    {
        Collider2D Ropehit = Physics2D.OverlapCircle(ropeCheck.position, RopeCheckRadius, Rope);

        if (Ropehit != null)
        {
            isRope = true;
            ropeInRange = Ropehit.attachedRigidbody;
        }
        else
        {
            isRope = false;
            ropeInRange = null;
        }
    }

    #endregion

    #region Player Dash Mechanism
    void CheckDash()
    {
        if(IsDashing)
        {
            Dashtext.text = " ";
            if (dashTimeLeft > 0)
            {
                rb.velocity = new Vector2(MovementInputDirection.x * DashSpeed, 0.0f);
                playerAnimator.SetBool("isDashing", true);
                dashTimeLeft -= Time.deltaTime;
            }
            else
            {
                IsDashing = false;
                playerAnimator.SetBool("isDashing", false);
            }
        }
        else
        {
            if(Time.time <= (lastDash + DashCoolDown))
            {
                Dashtext.text = "Dash Reacharging";
            }
            else
            {
                Dashtext.text = " ";
            }
            
        }
    }

    void AttemptToDash()
    {
        IsDashing = true;
        dashTimeLeft = DashTime;
        lastDash = Time.time;
    }

    #endregion

    #region Player Flipping mechanics
    void PlayerFlipping()   
    {
        float GunAngle = Gun.eulerAngles.z;

        //Determine facing Direction
        bool facingRight = GunAngle <= 90f || GunAngle >= 270f;

        //flip the player visually
        if (facingRight)
        {
            transform.localScale = StartingPos;
        }
        else
        {
            transform.localScale = new Vector2(-StartingPos.x, StartingPos.y);
        }
    }
    #endregion

    #region PLayer Rope Mechanism

    //Checking if player attached to rope if not any rope near let the player interact with a rope
    void CheckPlayerAttachedToRope()
    {
        if(!AttachedToRope && ropeInRange != null && isRope )
        {
            
            if (Time.time > detachTime + reattachDelay)
            {
                if(Input.GetKey(KeyCode.Mouse1))
                {
                    Debug.Log("Rope detected and Mouse1 pressed!");
                    if (AttachedTo != ropeInRange.transform.parent)
                    {
                        if(disregard == null || ropeInRange.transform.parent.gameObject != disregard)
                        {
                            Attach(ropeInRange);
                        }
                    }
                }
            }
        }
    }

    Rigidbody2D GetEndRopeSegment(Transform ropeParent, int offsetFromEnd = 0)
    {
        int count  = ropeParent.childCount;
        int Index = Mathf.Clamp(count - 1 - offsetFromEnd,0,count-1);
        Transform targetSegment = ropeParent.GetChild(Index);
        return targetSegment.GetComponent<Rigidbody2D>();
    }

    void Attach(Rigidbody2D RopeBone)
    {
        Transform ropeParent = RopeBone.transform.parent;
        Rigidbody2D targetSegment = GetEndRopeSegment(ropeParent, 1);

        hingejoint.connectedBody = targetSegment;
        hingejoint.enabled = true;
        AttachedToRope = true;
        AttachedTo = ropeParent;
    }

    void Detach()
    {
        AttachedToRope = false;
        hingejoint.enabled = false;
        hingejoint.connectedBody = null;
        AttachedTo = null;
        detachTime = Time.time;
        rb.velocity = new Vector2(MovementInputDirection.x * RopePushForce /25 , Jumpforce);
    }
    #endregion


    #region Gizmos for Debugging

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(GroundCheck.position, GroundCheckRadius);
        Gizmos.DrawWireSphere(ropeCheck.position, RopeCheckRadius);
    }

    #endregion

}
