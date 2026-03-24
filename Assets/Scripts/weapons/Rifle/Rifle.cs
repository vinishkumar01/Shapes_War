using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Rifle : MonoBehaviour, IUpdateObserver
{
    [Header("Reference")]
    [SerializeField] private WeaponSO _rifleAttributes;
    [SerializeField] private Transform _gunBarrel;
    [SerializeField] private TextMeshProUGUI _modeText;
    [SerializeField] private TextMeshProUGUI _bulletCountText;
    [SerializeField] private TextMeshPro _reloadText;
    [SerializeField] private Animator _muzzleFlashAnimator;
    private GunAiming _gunAimConfigs;

    private enum FireMode { FullAuto, Burst}
    private FireMode _fireMode = FireMode.FullAuto;

    [Header("recoil Force")]
    [SerializeField] private float _recoilForce = 5f;
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
    [SerializeField] private AudioClip _rifleSoundClip;

    private void OnEnable()
    {
        //Getting the gunAimConfigs here
        _gunAimConfigs = GetComponentInParent<GunAiming>();

        ////REgister this class which will use the update method
        UpdateManager.RegisterObserver(this);

        //Set the values when enabled
        _rifleAttributes.bulletsLeft = _rifleAttributes.magSize;
        _bulletCountText.text = _rifleAttributes.bulletsLeft.ToString();
        _rifleAttributes.autoReload = true;
        _rifleAttributes.isRifleCanShoot = true;
    }

    public void ObservedUpdate()
    {
        _gunAimConfigs.GunAim_with_CursorUI_To_World_Conversion();

        if (UserInputs.instance._playerInputs.Player.Fire.WasPressedThisFrame())
        {
            StartFiring();
        }
        else if(UserInputs.instance._playerInputs.Player.Fire.WasReleasedThisFrame())
        {
            _rifleAttributes.isRifleCanShoot = false;
        }

        //Input For switching Mode
        if(UserInputs.instance._playerInputs.Player.ModeSwitch.WasPressedThisFrame())
        {
            _fireMode++;

            if((int)_fireMode >= 2)
            {
                _fireMode = 0;
            }
        }

        //Input for Reloading
        if(UserInputs.instance._playerInputs.Player.Reload.WasPressedThisFrame())
        {
            if(!_rifleAttributes.isReloading)
            {
                StartCoroutine(Reload());
            }
        }
    }

    private void StartFiring()
    {
        //Break the method execution if the condition satisfies
        if (_rifleAttributes.isRifleCanShoot || _rifleAttributes.isReloading) return;
        _rifleAttributes.isRifleCanShoot = true;

        switch(_fireMode)
        {
            case FireMode.FullAuto:
                if (_modeText != null)
                    _modeText.text = "Full Auto";
                StartCoroutine(FireAuto());
                break;
            case FireMode.Burst:
                if (_modeText != null)
                    _modeText.text = "Burst";
                StartCoroutine(FireBurst());
                break;

        }
    }

    private IEnumerator FireAuto()
    {
        while(_rifleAttributes.isRifleCanShoot && _rifleAttributes.bulletsLeft > 0)
        {
            Shoot();
            yield return new WaitForSeconds(_rifleAttributes.fullAutoFireRate);
        }

        if (_rifleAttributes.bulletsLeft == 0 && _rifleAttributes.autoReload)
        {
            yield return Reload();
        }
    }

    private IEnumerator FireBurst()
    {
        int shotsFired = 0;

        while(shotsFired < _rifleAttributes.burstCount && _rifleAttributes.bulletsLeft > 0)
        {
            Shoot();
            shotsFired++;
            yield return new WaitForSeconds(_rifleAttributes.burstFireRate);
        }

        if(_rifleAttributes.bulletsLeft == 0 && _rifleAttributes.autoReload)
        {
            yield return Reload();
        }

        _rifleAttributes.isRifleCanShoot = false;
    }

    private void Shoot()
    {
        if (UserInputs.instance == null || UserInputs.instance._cursorTransform == null)
            return;

        if (Camera.main == null)
            return;

        if (_rifleAttributes.bulletsLeft <= 0 || _rifleAttributes.isReloading)
        {
            if (_rifleAttributes.bulletsLeft == 0 && _rifleAttributes.autoReload && !_rifleAttributes.isReloading)
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

        var RayHit = Physics2D.Raycast(origin, TargetDirection, _rifleAttributes.bulletRange, _rifleAttributes.hitLayer);

        Debug.DrawLine(origin, origin + TargetDirection * 100f, Color.magenta, .1f);


        //Instantiate bullets here 
        var bulletTrail = PoolManager.SpawnObject(_rifleAttributes.bulletPrefab, origin, Quaternion.identity, PoolManager.PoolType.GameObjects);
        _muzzleFlashAnimator.SetTrigger("Shoot");

        //We apply the recoil Force to Plyer Here
        Vector2 shootDirection = TargetDirection;
        shootDirection.y *= _player._isGrounded ? 0.5f : 0.1f; ; //reduce vertical kick
        shootDirection.Normalize();
        if (_player._isGrounded)
        {
            ApplyRecoilForce(shootDirection);
        }

        //Apply Camera Shake
        ApplyCameraShakeAndGamePlayPunch(shootDirection);

        //Applying Spuash effect to player
        if (_player.IsFacingRight)
        {
            _player._playerSquashandStretch.Squash(0.17f, -0.09f);
        }
        else
        {
            _player._playerSquashandStretch.Squash(-0.17f, -0.09f);
        }


        //Applying Gun Recoil
        _gunRecoil.RecoilKick(shootDirection);

        //Applying the pistol SFX:
        SFXManager._instance.playSFX(_rifleSoundClip, _gunBarrel.transform.position, 1f,false, false);

        //Micro Jitter 
        if (UnityEngine.Random.value > 0.5f)
        {
            _player._playerSquashandStretch.MicroJitter();
        }

        var trailScript = bulletTrail.GetComponent<BulletTracer>();

        if (RayHit.collider)
        {
            trailScript.initialize(origin, RayHit.point, RayHit);
        }
        else
        {
            var endPosition = origin + TargetDirection * _rifleAttributes.bulletRange;
            trailScript.initialize(origin, endPosition, new RaycastHit2D());
        }

        //Decrement the bullets at the end of the method 
        _rifleAttributes.bulletsLeft--;
        _bulletCountText.text = _rifleAttributes.bulletsLeft.ToString();
    }

    public IEnumerator Reload()
    {
        _rifleAttributes.isReloading = true;
        _reloadText.text = "RELOADING";
        yield return new WaitForSeconds(_rifleAttributes.reloadTime);
        _rifleAttributes.bulletsLeft = _rifleAttributes.magSize;
        //Debug.Log("Bullets Reloaded: " + BulletsLeft);
        _bulletCountText.text = _rifleAttributes.bulletsLeft.ToString();
        _rifleAttributes.isReloading = false;
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
        else if(_playerData.movementSpeed <= 5f)
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
        _rifleAttributes.isRifleCanShoot = false;
        _rifleAttributes.isReloading = false;
        StopAllCoroutines();

        //UnRegister
        UpdateManager.UnregisterObserver(this);
    }
}
