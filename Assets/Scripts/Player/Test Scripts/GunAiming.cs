using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Experimental.AI;
using UnityEngine.UI;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class GunAiming : MonoBehaviour , IUpdateObserver
{
    private Vector3 GunStartingPos;
    private GrapplingGunConfig grappleGun;
    private GrappleRopeConfigs grappleRope;
    private WeaponBehaviour weaponBehave;

    [Header("Weapon UI")]
    [SerializeField] private RectTransform RectImage;
    [SerializeField] private RectTransform[] weaponImage;

    [SerializeField] private GameObject player;
    [SerializeField] private Transform cursorWorldTarget;

    [Header("Cursor Limit")]
    [SerializeField] [Range(0, 30)]private float _limitRadius;

    #region GunAiming Attributes

    [Header("Transform Reference")]
    private Transform Player;
    [SerializeField] private Transform GunPivot;

    [Header("Player Aim Rotation")]
    [SerializeField] private bool rotateOverTime = false;
    [Range(0, 60)] [SerializeField] private float rotationSpeed = 4f;

    #endregion


    public enum WeaponType
    {
        Pistol,
        Rifle,
        GrapplingGun
    }

    private enum InputSource
    {
        Gamepad,
        Mouse
    }

    private InputSource _currentInputSource = InputSource.Mouse;

    public WeaponType weaponType = WeaponType.Pistol;

    private void OnEnable()
    {
        UpdateManager.RegisterObserver(this);
    }

    private void Awake()
    {
        grappleGun = GetComponentInChildren<GrapplingGunConfig>();
        grappleRope = GetComponentInChildren<GrappleRopeConfigs>();
        weaponBehave = GetComponentInChildren<WeaponBehaviour>();
    }

    // Start is called before the first frame update
    void Start()
    {
        GunStartingPos = GunPivot.localScale;
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        if(!grappleGun.enabled)
        {
            grappleGun._springJoint2D.enabled = false;
        }
    }

    public void ObservedUpdate()
    {
        GunPivot.transform.position = Player.transform.position;
        
        WeaponSelection();
        
        if(weaponType == WeaponType.Pistol)
        {
            weaponBehave.fireMode = WeaponBehaviour.FireMode.SemiAuto;
        }

        //Checking whether the player body is active and if not we are deactivating this gun pivot
        DestroyIfThePlayerBodyisNull();
    }


    public void GunAim(Vector3 lookPoint, bool allowRotationOverTime)
    {
        Vector3 PlayerArmDirection = lookPoint - GunPivot.position;

        //Calculating Angle in Degree
        float angle = Mathf.Atan2(PlayerArmDirection.y, PlayerArmDirection.x) * Mathf.Rad2Deg;

        if(rotateOverTime && allowRotationOverTime)
        {
            GunPivot.rotation = Quaternion.Lerp(GunPivot.rotation, Quaternion.AngleAxis(angle, Vector3.forward), Time.deltaTime * rotationSpeed);
        }
        else
        {
            GunPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    public void GunAim_with_CursorUI_To_World_Conversion()
    {

        //Converting cursor UI position to screen point
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, UserInputs.instance._cursorTransform.position);

        //Convert screen point to world point; set z for gun'z aiming plane
        float z = Mathf.Abs(GunPivot.position.z - Camera.main.transform.position.z);
        Vector3 cursorWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, z));

        if(cursorWorldTarget != null)
        {
            cursorWorldTarget.position = cursorWorldPos;
        }

        //Here we are gonna set the cursor Limit
        Vector2 playerToCursor = (cursorWorldPos - GunPivot.position).normalized;
        Vector2 cursorVector = playerToCursor * _limitRadius;
        Vector2 finalcursorPos = (Vector2)GunPivot.position + cursorVector;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 playerToMouse = (mousePosition - (Vector2)GunPivot.position).normalized;
        Vector2 finalMousePos = (Vector2)GunPivot.position + playerToMouse * _limitRadius;

        //Input from Gamepad
        Vector2 gamepadDelta; 
        if(Gamepad.current != null)
        {
            gamepadDelta = Gamepad.current.rightStick.ReadValue();
        }
        else
        {
            gamepadDelta = Vector2.zero;
        }
        Vector2 mouseDelta;
        if (Gamepad.current != null)
        {
            mouseDelta = Mouse.current.delta.ReadValue();
        }
        else
        {
            mouseDelta = Vector2.zero;
        }

        if (Gamepad.current != null && gamepadDelta.magnitude > 0.1f)
        {
            _currentInputSource = InputSource.Gamepad;
        }
        if(mouseDelta.magnitude > 2f)
        {
            _currentInputSource = InputSource.Mouse;
        }

        if(_currentInputSource == InputSource.Gamepad)
        {
            UserInputs.instance._cursorTransform.position = finalcursorPos;
            mousePosition = finalMousePos;
        }
        if(_currentInputSource == InputSource.Mouse)
        {
            Cursor.visible = false;
        }

        Debug.DrawLine(GunPivot.position, mousePosition, Color.yellow);

        GunAim(cursorWorldPos, true);
    }

    void WeaponsType()
    {
        switch(weaponType)
        {
            case WeaponType.GrapplingGun:
                grappleGun.GrapplingGunConfigs();
                break;
            case WeaponType.Pistol:
                Pistol();
                break;
            case WeaponType.Rifle:
                rifle();
                break;
        }
    }

    void WeaponSelection()
    {
        if (weaponImage != null && RectImage != null)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f) //Scroll Up
            {
                weaponType++;

                if ((int)weaponType >= System.Enum.GetValues(typeof(WeaponType)).Length)
                {
                    weaponType = 0;
                }
                UpdateWeaponUI();
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f || UserInputs.instance._playerInputs.Player.WeaponSwitch.IsPressed()) //Scroll Down
            {
                weaponType--;

                if ((int)weaponType < 0)
                {
                    weaponType = (WeaponType)System.Enum.GetValues(typeof(WeaponType)).Length - 1;
                }
                UpdateWeaponUI();
            }
        }
            

        if(weaponType == WeaponType.Pistol || weaponType == WeaponType.Rifle)
        {
            GunAim_with_CursorUI_To_World_Conversion();
        }

        //Debug.Log("CurrentWeapon: " + weaponType);

        if(grappleGun.enabled && weaponBehave.enabled)
        {
            WeaponsType();
        }
    }

    void Pistol()
    {
        if (UserInputs.instance._playerInputs.Player.Fire.WasPressedThisFrame())
        {
            weaponBehave.StartFiring();
        }
        else if (UserInputs.instance._playerInputs.Player.Fire.WasReleasedThisFrame())
        {
            weaponBehave.StopFiring();
        }
    }

    void rifle()
    {
        if(weaponBehave != null && weaponBehave.enabled)
        {
            if (UserInputs.instance._playerInputs.Player.Fire.WasPressedThisFrame())
            {
                weaponBehave.StartFiring();
            }
            else if (UserInputs.instance._playerInputs.Player.Fire.WasReleasedThisFrame())
            {
                weaponBehave.StopFiring();
            }
            if (UserInputs.instance._playerInputs.Player.ModeSwitch.WasPressedThisFrame())
            {
                weaponBehave.fireMode++;
                Debug.Log(weaponBehave.fireMode);

                if ((int)weaponBehave.fireMode >= 3)
                {
                    weaponBehave.fireMode = 0;
                }
            }
        }
        
    }

    void UpdateWeaponUI()
    {
        RectImage.position = weaponImage[(int)weaponType].position;
    }


    void DestroyIfThePlayerBodyisNull()
    {
        if(!player.activeInHierarchy)
        {
            gameObject.SetActive(false);
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(GunPivot.position, _limitRadius);
    }

    private void OnDisable()
    {
        UpdateManager.UnregisterObserver(this);
    }
}
