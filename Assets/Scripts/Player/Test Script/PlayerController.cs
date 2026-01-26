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
    [SerializeField] KnockBack knockBack;

    [Header("Camera Look Controller")]
    [SerializeField] private CameraLookController cameraLook;

    [Header("Gravity Configs")]
    [SerializeField]float gravityStrength; //Downwards force (gravity) needed for the desired jumpHeight and jumpTimeToApex.
    [SerializeField]public float gravityScale; //Strength of the player's gravity as a multiplier of gravity (set in ProjectSettings/Physics2D).
    [SerializeField] float fallGravityMult; //Multiplier to the player's gravityScale when falling.
    [SerializeField] float MaxFallSpeed; //Maximum fall speed (terminal velocity) of the player when falling.
    [SerializeField] float fastFallGravityMult; //Larger multiplier to the player's gravityScale when they are falling and a downwards input is pressed.
//Seen in games such as Celeste, lets the player fall extra fast if they wish.
    [SerializeField] float MaxFastFallSpeed; //Maximum fall speed(terminal velocity) of the player when performing a faster fall.  

    [Header("Jump Configs")]
    [SerializeField] float Jumpforce;
    [SerializeField] float JumpHeight;
    [SerializeField] float JumpTimeToApex;
    [SerializeField] bool isJumping;
    
    [Header("Mario Jump Effect")]
    [SerializeField] float jumpTime = 0.35f;
    [SerializeField] float jumpTimeCounter;

    [Header("Coyote Time")]
    [SerializeField] float coyoteTime = 0.2f;
    [SerializeField] float coyoteTimeCounter;

    [Header("Jump Buffer")]
    [SerializeField] float jumpBufferTime = 0.2f;
    [SerializeField] float jumpBufferTimeCounter;

    [Header("Double Jump")]
    [SerializeField] float doubleJumpForce;
    [SerializeField] bool doubleJump;
    [SerializeField] bool doubleJumpSkill = true;

    [Header("Movement Configs")]
    [SerializeField]float MovementSpeed = 10.0f;
    [SerializeField]float MovementInputXDirection;
    [SerializeField] float MovementInputYDirection;
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
    [SerializeField] float RopeSwingForce = 8f;
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
        knockBack = GetComponent<KnockBack>();

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
        if (!knockBack.IsBeingKnockedBack)
        {
            CheckDash();
            CheckPlayerAttachedToRope();
            CheckInputs();
            PlayerJumps();
        }
    }

    private void FixedUpdate()
    {
        
        ApplyMovements();
        PlayerRopeSwing();
        CheckSurrounding();
        RopeCheck();
        PlayerFlipping();
        GravityConfigs();

    }

    #region Checking player Inputs

    void CheckInputs()
    {
        //Horizontal Movements
        MovementInputXDirection = UserInputs.instance.moveInputs.x;
        MovementInputYDirection = UserInputs.instance.moveInputs.y;

        //If player Attached to Rope then the user can press space or jump button to detatch from the Rope
        if(UserInputs.instance._playerInputs.Player.Jump.WasPressedThisFrame())
        {
            if (AttachedToRope)
            {
                Detach();
            }
        }
        
        //Player Dash
        if (UserInputs.instance._playerInputs.Player.Dash.WasPressedThisFrame())
        {
            if(Time.time >= (lastDash + DashCoolDown))
            {
               AttemptToDash(); 
            }
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

        //
        if (rb.velocity.y < 0 && MovementInputYDirection < 0)
        {
            // Much Higher gravity pull if Holding Down
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
        //Setting up the coyote Time so that the player can jump after leaving the platform for fraction of seconds
        if(isGrounded)
        {
            coyoteTimeCounter = coyoteTime;

            //DoubleJump
            doubleJump = true;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        //Setting up the jump buffer time so that the player can press jump before reaching the ground and still jump
        if(UserInputs.instance._playerInputs.Player.Jump.WasPressedThisFrame())
        {
           jumpBufferTimeCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferTimeCounter -= Time.deltaTime;
        }

        //Jump button was pushed this frame
        if(jumpBufferTimeCounter > 0f && coyoteTimeCounter > 0f)
        {
            Debug.Log("Jump was pressed");
            isJumping = true;
            jumpTimeCounter = jumpTime;
            rb.velocity = new Vector2(rb.velocity.x, Jumpforce);

            //reset the jump Buffer Counter 
            jumpBufferTimeCounter = 0f;
        }

        else if(jumpBufferTimeCounter > 0f && !isGrounded && coyoteTimeCounter <= 0f && doubleJump && doubleJumpSkill)
        {
            isJumping = true ;
            jumpTimeCounter = jumpTime;

            rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);

            doubleJump = false;

            //reset the jump Buffer Counter 
            jumpBufferTimeCounter = 0f;
        }

        //Jump button been held
        if(UserInputs.instance._playerInputs.Player.Jump.IsPressed())
        {
            Debug.Log("Jump was held down");
            if (jumpTimeCounter > 0 && isJumping)
            {
                rb.velocity = new Vector2(rb.velocity.x, Jumpforce);
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }
        //Jump button was Released
        if(UserInputs.instance._playerInputs.Player.Jump.WasReleasedThisFrame())
        {
            isJumping = false;
            coyoteTimeCounter = 0f;
        }

        if(!isJumping && isGrounded)
        {
            jumpTimeCounter = jumpTime;
        }
    }
    #endregion

    #region Player Horizontal Movements
    void ApplyMovements()
    {
        float TargetVelocityX = MovementSpeed * MovementInputXDirection;

        rb.velocity = new Vector2(Mathf.SmoothDamp(rb.velocity.x, TargetVelocityX, ref VelocityXSmoothing, (isGrounded)? accelerationtimeGrounded : accelerationtimeAirborne), rb.velocity.y);

        playerAnimator.SetInteger("isPlayerMoving", Mathf.Abs((int)TargetVelocityX));
    }

    void PlayerRopeSwing()
    {
        if (AttachedToRope)
        {
            rb.AddRelativeForce(new Vector2(MovementInputXDirection * RopeSwingForce, rb.velocity.y));
        }
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
                rb.velocity = new Vector2(MovementInputXDirection * DashSpeed, 0.0f);
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


        cameraLook.SetFacing(facingRight);
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
                if(UserInputs.instance._playerInputs.Player.PerformAction.WasPressedThisFrame())
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
        rb.velocity = new Vector2(MovementInputXDirection * RopeSwingForce , Jumpforce);
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
