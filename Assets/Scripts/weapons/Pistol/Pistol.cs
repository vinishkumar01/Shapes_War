using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class Pistol : MonoBehaviour, IUpdateObserver
{
    [Header("Reference")]
    [SerializeField] private WeaponSO _pistolAttributes;
    [SerializeField] private Transform _gunBarrel;
    [SerializeField] private TextMeshPro _reloadText;
    [SerializeField] private Animator _muzzleFlashAnimator;
    private GunAiming _gunAimConfigs;

    [Header("recoil Force")]
    [SerializeField] private float _recoilForce = 2f;
    [SerializeField] private Player _player;

    [Header("Camera shake and bumpiness")]
    [SerializeField] private float _bumpStrength = 0.12f;
    [SerializeField] private float _punchShakeStrength = 0.08f;
    [SerializeField] private float _punchShakeStrengthBackGround = 0.08f;


    [Header("Player SO Data")]
    [SerializeField] private PlayerDataSO _playerData;

    [Header("Gun Recoil Config")]
    [SerializeField] private GunRecoil _gunRecoil;


    [Header("SFX")]
    [SerializeField]private AudioClip _pistolSoundClip;

    [Header("LayerMask")]
    //Taking reference of the platform layer to check the distance to find can shoot or not;
    [SerializeField] private LayerMask _platformLayer;

    private void OnEnable()
    {
        //Getting the gunAimConfigs here
        _gunAimConfigs = GetComponentInParent<GunAiming>();

        //REgister this class which will use the update method
        UpdateManager.RegisterObserver(this);

        //Set the values when enabled
        _pistolAttributes.bulletsLeft = _pistolAttributes.magSize;
        _pistolAttributes.autoReload = true;
        _pistolAttributes.isPistolCanShoot = true;
        UIManager.InvokeBulletAndFireModeUpdate(_pistolAttributes.bulletsLeft, "SEMI - AUTO");
    }

    public void ObservedUpdate()
    {
        if(!GameState.CanPlayerControl)
        {
            return;
        }

        _gunAimConfigs.GunAim_with_CursorUI_To_World_Conversion();


        if (UserInputs.instance._playerInputs.Player.Fire.WasPressedThisFrame())
        {
            Shoot();
        }
        else if(UserInputs.instance._playerInputs.Player.Fire.WasReleasedThisFrame())
        {
            _pistolAttributes.isPistolCanShoot = false;
        }

        if(UserInputs.instance._playerInputs.Player.Reload.WasPressedThisFrame())
        {
            if(!_pistolAttributes.isReloading)
            {
                StartCoroutine(Reload());
            }
        }
    }

    private void Shoot()
    {

        if (UserInputs.instance == null || UserInputs.instance._cursorTransform == null)
            return;

        if (Camera.main == null)
            return;

        //Break the method execution if the condition satisfies
        if (_pistolAttributes.isPistolCanShoot || _pistolAttributes.isReloading)
        {
            return;
        } 

        _pistolAttributes.isPistolCanShoot = true;

        if(_pistolAttributes.bulletsLeft <= 0 || _pistolAttributes.isReloading)
        {
            if(_pistolAttributes.bulletsLeft == 0 && _pistolAttributes.autoReload && !_pistolAttributes.isReloading)
            {
                //if bullet reaches 0 and autoreload is set to true and if not reloading this coroutine is triggered
                StartCoroutine(Reload());
            }
            return;
        }

        //Shoot mechanism

        //get the cursor transform by converting from world to screen
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, UserInputs.instance._cursorTransform.position);
        float z = _gunBarrel.position.z - Camera.main.transform.position.z;
        Vector3 cursorWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, z));

        //get the firePoint of the gun 
        Vector3 origin = _gunBarrel.position;
        Vector3 TargetDirection = (cursorWorldPos - origin).normalized;

        Vector3 wallCheckOrigin = _gunBarrel.position - TargetDirection * 0.5f;
        RaycastHit2D wallCheck = Physics2D.Raycast(wallCheckOrigin, TargetDirection, 1.5f, _platformLayer);

        if(wallCheck.collider != null)
        {
            //Gun is too close to the wall, dont shoot
            return;
        }

        var RayHit = Physics2D.Raycast(origin, TargetDirection, _pistolAttributes.bulletRange, _pistolAttributes.hitLayer);

        Debug.DrawLine(origin, origin + TargetDirection * 100f, Color.magenta, .1f);


        //Instantiate bullets here 
        var bulletTrail = PoolManager.SpawnObject(_pistolAttributes.bulletPrefab, origin, Quaternion.identity, PoolManager.PoolType.GameObjects);
        _muzzleFlashAnimator.SetTrigger("Shoot");

        //We apply the recoil Force to Plyer Here
        Vector2 shootDirection = TargetDirection;
        shootDirection.y *= _player._isGrounded ? 0.5f : 0.1f; ; //reduce vertical kick
        shootDirection.Normalize();
        if(_player._isGrounded)
        {
            ApplyRecoilForce(shootDirection);
        }
        
        //Apply Camera Shake
        ApplyCameraShakeAndGamePlayPunch(shootDirection);

        //Applying Spuash effect to player
        if(_player.IsFacingRight)
        {
            _player._playerSquashandStretch.Squash(0.13f, -0.09f);
        }
        else
        {
            _player._playerSquashandStretch.Squash(-0.13f, -0.09f);
        }
        

        //Applying Gun Recoil
        _gunRecoil.RecoilKick(shootDirection);

        //Applying the pistol SFX:
        SFXManager._instance.playSFX(_pistolSoundClip, _gunBarrel.transform.position, 1f,false, false);

        //Micro Jitter 
        if (UnityEngine.Random.value > 0.5f)
        {
            _player._playerSquashandStretch.MicroJitter();
        }

        var trailScript = bulletTrail.GetComponent<BulletTracer>();

        if(RayHit.collider)
        {
            trailScript.initialize(origin, RayHit.point, RayHit);
        }
        else
        {
            var endPosition = origin + TargetDirection * _pistolAttributes.bulletRange;
            trailScript.initialize(origin, endPosition, new RaycastHit2D());
        }

        //Decrement the bullets at the end of the method 
        _pistolAttributes.bulletsLeft--;
        UIManager.InvokeBulletAndFireModeUpdate(_pistolAttributes.bulletsLeft, "SEMI - AUTO");
    }

    public IEnumerator Reload()
    {
        _pistolAttributes.isReloading = true;
        _reloadText.text = "RELOADING";
        yield return new WaitForSeconds(_pistolAttributes.reloadTime);
        _pistolAttributes.bulletsLeft = _pistolAttributes.magSize;
        //Debug.Log("Bullets Reloaded: " + BulletsLeft);
        UIManager.InvokeBulletAndFireModeUpdate(_pistolAttributes.bulletsLeft, "SEMI - AUTO");
        _pistolAttributes.isReloading = false;
        _reloadText.text = " ";
    }

    private void ApplyRecoilForce(Vector2 shootDirection)
    {
        if (_player._isDashing)
        {
            return;
        }

        float recoilMultiplier = 0f;

        if (_playerData.movementSpeed > 10f)
        {
            //Cutting out recoil as much as possible
            recoilMultiplier = 0.2f;
        }
        else if (_playerData.movementSpeed <= 5f)
        {
            recoilMultiplier = 0.5f;
        }

        _player.RB.AddForce(-shootDirection * _recoilForce * recoilMultiplier, ForceMode2D.Impulse);
    }

    private void ApplyCameraShakeAndGamePlayPunch(Vector2 shootDirection)
    {

        //Applying Gamplay Punch
        if (_player._isGrounded)
        {
            //Here we apply the Camera shake and set the bumpiness of the camera
            CameraShakeController.instance.CameraBump(-shootDirection, _bumpStrength); //camBumpXY

            float movefactor = Mathf.InverseLerp(0, _playerData.maxMovementSpeed, _playerData.movementSpeed);

            //When moving fast punch almost gone
            float punchStrength = Mathf.Lerp(_punchShakeStrength, _punchShakeStrength * 0.001f, movefactor);
            float punchStrengthBG = Mathf.Lerp(_punchShakeStrengthBackGround, _punchShakeStrengthBackGround * 0.001f, movefactor);

            GameplayPunch.instance.Punch(-shootDirection, punchStrength);
            GamePlayPunchForBackGrounds.instance.Punch(-shootDirection, punchStrengthBG);
        }

    }

    private void OnDisable()
    {
        _pistolAttributes.isPistolCanShoot = false;
        _pistolAttributes.isReloading = false;
        StopAllCoroutines();

        //UnRegister
        UpdateManager.UnregisterObserver(this);
    }
}
