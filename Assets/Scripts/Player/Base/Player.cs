using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour, IPlayerDamageable, IUpdateObserver, IFixedUpdateObserver, ILateUpdateObserver
{
    #region Player Attributes

    public int MaxHealth { get; set; }
    [field: SerializeField] public int CurrentHealth { get; set; }

    [Header("References")]
    private HealthBar _healthBar;
    private TextMeshPro _healthText;
    [HideInInspector] public KnockBack _knockBack;
    [HideInInspector] public PlayerFlashAndDissolveEffect _flashEffect;
    private PlayerDeadParticlesInitiation _deadParticlesInitiation;
    [SerializeField] private GameObject _gunHolder;
    private GunAiming _gunAiming;
    [SerializeField] private FullScreenEffectController _fullScreenEffectController;
    [SerializeField] public GameObject _shockWaveScreen;
    [SerializeField] private WeaponSO _pistolAttributes;
    [SerializeField] private WeaponSO _rifleAttributes;
    [SerializeField] private Pistol _pistol;
    [SerializeField] private Rifle _rifle;

    [Header("player SFX")]
    public AudioSource _audioSource;
    public AudioClip _slideSoundClip;
    public AudioClip _jumpSoundClip;
    public AudioClip _dashSoundClip;
    [SerializeField] private AudioClip _hurtSoundEffect;
    [SerializeField] private AudioClip _deathSoundEffect;


    public Animator _animator;
    public Rigidbody2D RB { get; set; }
    public bool IsFacingRight = true;

    [Header("Surrounding and Rope Check")]
    public bool _isGrounded;
    [SerializeField]private Transform _groundCheck;
    [SerializeField] private LayerMask _groundLayer;

    public bool _isRope;
    [SerializeField] private Transform _ropeCheck;
    [SerializeField] private LayerMask _ropelayer;

    [Header("Rope Configs")]
    private HingeJoint2D _hingeJoint;
    private Transform _attachedTo;
    private GameObject _disregard;
    private Rigidbody2D _ropeInRange = null;
    public bool _attachedToRope;
    private float _detachTime;

    [Header("Camera Look Controller")]
    [SerializeField] private CameraLookController _cameraLookController;

    [Header("Facing Direction")]
    [SerializeField] private Transform _gun;
    private Vector3 _startingPos;

    [Header("Gravity Configs")]
    [SerializeField] private float _gravityStrength; //Downwards force (gravity) needed for the desired jumpHeight and jumpTimeToApex.
    public float _gravityScale; //Strength of the player's gravity as a multiplier of gravity (set in ProjectSettings/Physics2D).

    [Header("Jump configs")]
    public float _jumpForce;
    public bool _isJumping;

    [Header("Player UI")]
    public TextMeshPro _dashText;

    [Header("Mario Jump Effect")]
    public float _jumpTimeCounter;

    [Header("Coyote Time")]
    public float _coyoteTimeCounter;

    [Header("Jump Buffer")]
    public float _jumpBufferTimeCounter;

    [Header("Double Jump")]
    public bool _doubleJump;

    [Header("Dash Configs")]
    public bool _isDashing;
    public float _lastImageXpos;
    public float _distancebetweenImages;
    public GameObject _playerAfterImage;

    [Header("Power Ups")]
    [Header("UI")]
    public TextMeshProUGUI _doubleJumpSkill;
    public TextMeshProUGUI _doubleJumpCountUI;
    public TextMeshProUGUI _dashSkill;
    public TextMeshProUGUI _dashCountUI;
    public TextMeshProUGUI _grappleAmmoUI;

    //to Store the last Death Position of the player 
    public Vector3 lastDeathPosition { get; private set; }

    [Header("Player Visuals")]
    [SerializeField] private GameObject _playerVisuals;
    public SquashAndStretch _playerSquashandStretch;

    #endregion

    #region State Machine Variables

    public PlayerStateMachine _playerStateMachine { get; set; }

    public PlayerIdleState _playerIdleState { get; set; }
    public PlayerMoveState _playerMoveState { get; set; }
    public PlayerJumpState _playerJumpState { get; set; }
    public PlayerDashState _playerDashState { get; set; }

    #endregion

    [Header("Player Data")]
    public PlayerDataSO _playerDataSO;

    private void OnEnable()
    {
        UpdateManager.RegisterObserver(this);
        FixedUpdateManager.RegisterObserver(this);
        LateUpdateManager.RegisterObserver(this);

        //Making sure that the player has the weapon and the sprite when in the scene
        CheckIfGunisPresentWithPlayer();
        CheckIfPlayerVisualisPresent();

        //Reset the Audio Source (Stopping the audio its playing)
        _audioSource.Stop();
    }

    private void CheckIfGunisPresentWithPlayer()
    { 
        if (!_gunHolder.activeInHierarchy)
        {
            _gunHolder.SetActive(true);
        }
    }

    private void CheckIfPlayerVisualisPresent()
    {
        if (!_playerVisuals.activeInHierarchy)
        {
            _playerVisuals.SetActive(true);
        }
    }

    private void Awake()
    {
        _healthText = GetComponentInChildren<TextMeshPro>();

        _playerStateMachine = new PlayerStateMachine();

        _playerIdleState = new PlayerIdleState(this, _playerStateMachine, _playerDataSO);
        _playerMoveState = new PlayerMoveState(this, _playerStateMachine, _playerDataSO);
        _playerJumpState = new PlayerJumpState(this, _playerStateMachine, _playerDataSO);
        _playerDashState = new PlayerDashState(this, _playerStateMachine, _playerDataSO);
    }

    private void AssignHealthAttributes()
    {
        if (GameManager._instance._gameManagerSpawnListSO.player == null)
        {
            Debug.LogWarning("---Player is null---");
        }

        GameObject playerPrefab = GameManager._instance.GetPlayerPrefab();

        if(GameManager._instance != null && GameManager._instance.TryGetPlayerData(playerPrefab, out var data))
        {
            MaxHealth = data.playerMaxHealth;
        }
    }

    public void ResetPlayerHealth()
    {
        //Player health
        CurrentHealth = MaxHealth;

        UpdateHealth();

        //Lets Reset the health text here 
        if (_healthText != null)
        {
            ResetHealthText(MaxHealth);
        }

        //Disable the shockWave when resetting the player health too
        //_shockWaveScreen.SetActive(false);
    }

    private void Start()
    {
        AssignHealthAttributes();

        //Player health
        CurrentHealth = MaxHealth;

        //Lets Reset the health text here 
        if (_healthText != null)
        {
            ResetHealthText(MaxHealth);
        }

        RB = GetComponent<Rigidbody2D>();
        
        //Player health References
        _healthBar = GetComponentInChildren<HealthBar>();
        _flashEffect = GetComponent<PlayerFlashAndDissolveEffect>();
        _knockBack = GetComponent<KnockBack>();
        _deadParticlesInitiation = GetComponent<PlayerDeadParticlesInitiation>();

        //Player Visual Reference
        _playerSquashandStretch = _playerVisuals.GetComponent<SquashAndStretch>();

        //UI
        _doubleJumpSkill = GameObject.FindGameObjectWithTag("DJSUI").GetComponent<TextMeshProUGUI>();
        _doubleJumpCountUI = GameObject.FindGameObjectWithTag("DJCUI").GetComponent<TextMeshProUGUI>();
        _dashSkill = GameObject.FindGameObjectWithTag("DSUI").GetComponent<TextMeshProUGUI>();
        _dashCountUI = GameObject.FindGameObjectWithTag("DSCUI").GetComponent<TextMeshProUGUI>();
        _grappleAmmoUI = GameObject.FindGameObjectWithTag("GrappleCount").GetComponent<TextMeshProUGUI>();


        //Player Hinge Joint Configs for Rope Config
        _hingeJoint = GetComponent<HingeJoint2D>();

        //Player Starting Position
        _startingPos = _playerVisuals.transform.localScale;

        //Initializing the default state for the player
        _playerStateMachine.Initialize(_playerIdleState);
    }

    #region Player Health configs
    public void Damage(int damageAmount, Vector2 hitDirection, Vector2 hitPoint, Vector2 hitNormal)
    {
        if (CurrentHealth <= 0) return;

        CurrentHealth -= damageAmount;

        //Update Health Bar
        UpdateHealth();

        //KnockBack
        _knockBack.CallKnockBackCoroutine(hitDirection, Vector2.up, Input.GetAxisRaw("Horizontal"));

        //Hurt Sound Effect 
        SFXManager._instance.playSFX(_hurtSoundEffect, transform.position, 1f, false, false);

        //Damage Flash
        _flashEffect.CallDamageFlash();

        //Spawn Blood Effect
        BloodFXController.instance.PlayBloodFX(hitPoint, hitNormal, hitDirection, BloodStainSpawnManager.CharacterType.Player);

        if (CurrentHealth <= 0)
        {
            Die();
            return;
        }
    }

    public void UpdateHealth()
    {
        _healthBar.UpdateHealthBar(MaxHealth, CurrentHealth);
        _healthText.text = CurrentHealth.ToString();
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
        //lets Stop the Coroutines for the Knock back and the flash Effect
        _knockBack.StopKnockBack(); // This method contains the logic for stop coroutine and we are setting isBeingKnockedBack to false so that we dont continue on executing the coroutine

        _flashEffect.StopFlashEffect(); // This method contains the same logic as the StopKnockBack and also we are resetting the flash amount 0 here.
        _flashEffect.StopDashFlashEffect();

        //Death Sound Effect
        SFXManager._instance.playSFX(_deathSoundEffect, transform.position, 1f, false, false);

        //Call the Body particles
        _deadParticlesInitiation.CallSpawnBodyParticle();

        lastDeathPosition = transform.position; 
        _playerDataSO.lives--;
        Debug.Log($"Player Lives: {_playerDataSO.lives}");

        if(_playerDataSO.lives > 0)
        {
            GameManager._instance.OnPlayerDiedButHasLives(this);
        }
        else
        {
            GameManager._instance.ONPlayerGameOver(this);
        }

       gameObject.SetActive(false);
    }

    #endregion

    #region Inputs for Players

    public float MovementInputXDirection => UserInputs.instance.moveInputs.x;

    public float MovementInputYDirection => UserInputs.instance.moveInputs.y;

    public bool JumpPressed => UserInputs.instance._playerInputs.Player.Jump.WasPressedThisFrame();
    public bool JumpHeld => UserInputs.instance._playerInputs.Player.Jump.IsPressed();
    public bool JumpReleased => UserInputs.instance._playerInputs.Player.Jump.WasReleasedThisFrame();
    public bool DashPressed => UserInputs.instance._playerInputs.Player.Dash.WasPressedThisFrame();

    #endregion

    public void ObservedUpdate()
    {
        //Check for player Health t upgrade in every wave
        GameManager._instance.CheckPlayerHealthEveryFrame(this);
        Debug.Log($" Regular print: Maxhealth: {MaxHealth}, Current Health: {CurrentHealth}");

        CheckPlayerAttachedToRope();
        JumpCounters();

        if (UserInputs.instance != null)
        {
            PlayerFacing();
        }

        #region FullScreen Low Health Effect
        //Check If the current health is below 30 or 40 we will enable the fullScreen Effect to indicate the player
        if (CurrentHealth <= 40 && !_fullScreenEffectController._lowHealthEffectActive)
        {
            _fullScreenEffectController._lowHealthEffectActive = true;

            _fullScreenEffectController.lowHealthEffectCoroutine = StartCoroutine(_fullScreenEffectController.LowHealthEffect());
        }
        else if(CurrentHealth >= 42 && _fullScreenEffectController._lowHealthEffectActive)
        {
            _fullScreenEffectController._lowHealthEffectActive = false;

            if(_fullScreenEffectController.lowHealthEffectCoroutine != null)
            {
                StopCoroutine(_fullScreenEffectController.LowHealthEffect());
                _fullScreenEffectController.lowHealthEffectCoroutine = null;
            }

            _fullScreenEffectController.ResetLowHealthEffect();
        }
        #endregion

        _playerStateMachine._currentPlayerState.FrameUpdate();
    }

    public void ObservedFixedUpdate()
    {
        CheckSurroundingAndRope();
        GravityConfigs();

        _playerStateMachine._currentPlayerState.PhysicsUpdate();
    }

    public void ObservedLateUpdate()
    {
        _playerStateMachine._currentPlayerState.LateFrameUpdate();
    }

    #region Player gravity configs

    private void GravityConfigs()
    {
        //Calculating the gravity Strength using the below formula
        //Physics2D.gravity is been set to -30, jump height = 3, jumpTimeToApex = 0.2;
        _gravityStrength = -(2 * _playerDataSO.jumpHeight) / (_playerDataSO.jumpTimeToApex * _playerDataSO.jumpTimeToApex);

        //calculate the rigidBody's gravity Scale (i.e. gravity strength relative to Unity's gravity value)
        _gravityScale = _gravityStrength / Physics2D.gravity.y;

        //Calculate the jumpForce using the formula (InitialJumpVelocity = gravity * timeToJumpApex)
        _jumpForce = Mathf.Abs(_gravityStrength) * _playerDataSO.jumpTimeToApex;

        //Apply Force vertically when Falling
        if (RB.velocity.y < 0 && MovementInputYDirection < 0) 
        {
            //Much higher gravity pull if holding down
            SetGravityScale( _gravityScale * _playerDataSO.fastFallGravityMult);
            //lets Cap the maximum fast fall speed, so when falling over large distance we dont accelerate to insane speed
            RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -_playerDataSO.maxFastFallSpeed));
        }
        else if(RB.velocity.y < 0)
        {
            //Higher gravity if falling 
            SetGravityScale(_gravityScale * _playerDataSO.fallGravityMult);

            //lets cap the maximum fall speed, so when falling over larger distance we dont accelerate to insanly
            RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -_playerDataSO.maxFallSpeed));
        }
        else
        {
            SetGravityScale(_gravityScale);
        }
    }

    private void SetGravityScale(float Scale)
    {
        RB.gravityScale = Scale;
    }

    #endregion

    #region Player surrounding Configs
    private void CheckSurroundingAndRope()
    {
        _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _playerDataSO.groundCheckRadius, _groundLayer);

        Collider2D ropeHit = Physics2D.OverlapCircle(_ropeCheck.position, _playerDataSO.ropeCheckRadius, _ropelayer);

        if (ropeHit != null)
        {
            _isRope = true;
            _ropeInRange = ropeHit.attachedRigidbody;
        }
        else
        {
            _isRope = false;
            _ropeInRange = null;
        }
    }
    #endregion

    #region Player Rope Configs

    private void CheckPlayerAttachedToRope()
    {
        if(!_attachedToRope && _ropeInRange != null && _isRope)
        {
            if(Time.time > _detachTime + _playerDataSO.reattachDelay)
            {
                if(UserInputs.instance._playerInputs.Player.PerformAction.WasPressedThisFrame())
                {
                    if(_attachedTo != _ropeInRange.transform.parent)
                    {
                        if(_disregard == null || _ropeInRange.transform.parent.gameObject != _disregard)
                        {
                            Attach(_ropeInRange);
                        }
                    }
                }
            }
        }
    }

    private Rigidbody2D GetEndSegmentOfRope(Transform ropeParent, int offsetFromEnd = 0)
    {
        int count = ropeParent.childCount;
        int index = Mathf.Clamp(count - 1 - offsetFromEnd, 0, count - 1);

        Transform targetSegment = ropeParent.GetChild(index);
        return  targetSegment.GetComponent<Rigidbody2D>();
    }

    private void Attach(Rigidbody2D ropeBone)
    {
        Transform ropeParent = ropeBone.transform.parent;
        Rigidbody2D targetSegment = GetEndSegmentOfRope(ropeParent, 1);

        _hingeJoint.connectedBody = targetSegment;
        _hingeJoint.enabled = true;
        _attachedToRope = true;
        _attachedTo = ropeParent;
    }

    public void Detach()
    {
        _hingeJoint.connectedBody = null;
        _hingeJoint.enabled = false;
        _attachedToRope = false;
        _attachedTo = null;
        _detachTime = Time.time;

        //Apply force while detachig from rope in both x and y
        RB.velocity = new Vector2(RB.velocity.x, _jumpForce);
    }

    #endregion

    #region Player Facing Direction configs

    private void PlayerFacing()
    {
        if (UserInputs.instance == null || UserInputs.instance._cursorTransform == null)
            return;

        if (Camera.main == null)
            return;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, UserInputs.instance._cursorTransform.position);
        float z = transform.position.z - Camera.main.transform.position.z;

        Vector3 cursorWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, z));
        Vector3 playerToCursor = cursorWorldPos - transform.position;
        IsFacingRight = playerToCursor.x > 0;

        //Apply Flip
        Vector3 targetScale = IsFacingRight ? _startingPos : new Vector3(-_startingPos.x, _startingPos.y, _startingPos.z);
        _playerVisuals.transform.localScale = targetScale;
        _cameraLookController.SetFacing(IsFacingRight);
    }

    #endregion

    #region Animation triggers and Shoot Animation

    public void AnimationTriggerEvent(AnimationTriggerType triggerType)
    {
      _playerStateMachine._currentPlayerState.AnimationTriggerEvent(triggerType);
    }

    public enum AnimationTriggerType
    {
        Idle,
        Run,
        Jump,
        Dash
    }

    #endregion

    #region Player Jump Counters

    private void JumpCounters()
    {
        if(_isGrounded)
        {
            _coyoteTimeCounter = _playerDataSO.coyoteTime;

            //Double Jump
            _doubleJump = true;
        }
        else
        {
            _coyoteTimeCounter -= Time.deltaTime;
        }

        if (JumpPressed)
        {
            _jumpBufferTimeCounter = _playerDataSO.jumpBufferTime;   
        }
        else
        {
            _jumpBufferTimeCounter -= Time.deltaTime;
        }

        if(_isGrounded)
        {
            _jumpTimeCounter = _playerDataSO.jumpTime;
        }
    }

    #endregion

    #region Debugging
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(_groundCheck.position, _playerDataSO.groundCheckRadius);
        Gizmos.DrawWireSphere(_ropeCheck.position, _playerDataSO.ropeCheckRadius);
    }

    #endregion

    private void OnDisable()
    {
        UpdateManager.UnregisterObserver(this);
        FixedUpdateManager.UnregisterObserver(this);
        LateUpdateManager.UnregisterObserver(this); 

        StopAllCoroutines();
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(650, 10, 500, 300), $"Current State: {_playerStateMachine._currentPlayerState?.GetType().Name}");
    }

    #region PowerUp Interactions

    //PowerUps Interactions
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("JumpPowerUp"))
        {
            _playerDataSO.doubleJumpSkill = true;
            _doubleJumpSkill.text = _playerDataSO.doubleJumpSkill.ToString();

            _playerDataSO.doubleJumpCount += 5;
            _doubleJumpCountUI.text = _playerDataSO.doubleJumpCount.ToString();

            PoolManager.ReturnObjectToPool(collision.gameObject, PoolManager.PoolType.GameObjects);
            
        }

        if(collision.CompareTag("DashPowerUp"))
        {
            _playerDataSO.dashSkill = true;
            _dashSkill.text = _playerDataSO.dashSkill.ToString();

            _playerDataSO.dashCount += 5;
            _dashCountUI.text = _playerDataSO.dashCount.ToString();

            PoolManager.ReturnObjectToPool(collision.gameObject, PoolManager.PoolType.GameObjects);
        }

        if (collision.CompareTag("GrappleGunBullet"))
        {
            _playerDataSO._grappleAmmo += 5;
            _grappleAmmoUI.text = _playerDataSO._grappleAmmo.ToString();

            PoolManager.ReturnObjectToPool(collision.gameObject, PoolManager.PoolType.GameObjects);
        }

        if(collision.CompareTag("HealthPack"))
        {
            GameManager._instance.AddHealthToPlayer(this);
            UpdateHealth();
            Debug.Log($"[From Trigger Event]: CurrentHealth: {CurrentHealth}");

            PoolManager.ReturnObjectToPool(collision.gameObject, PoolManager.PoolType.GameObjects);

        }
    }

    #endregion
}
