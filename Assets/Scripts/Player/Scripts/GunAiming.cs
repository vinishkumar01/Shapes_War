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
    [SerializeField] private GrappleGun grappleGunConfig;

    [SerializeField] private GameObject player;
    [SerializeField] private Transform cursorWorldTarget;
    public Vector3 _cursorWorldPos { get; private set; }

    [Header("Cursor Limit")]
    [SerializeField] [Range(0, 30)]private float limitRadius;

    [SerializeField] private Transform _gunHolder;

    #region GunAiming Attributes

    [Header("Transform Reference")]
    private Transform Player;
    [SerializeField] private Transform GunPivot;

    [Header("Player Aim Rotation")]
    [SerializeField] private bool rotateOverTime = false;
    [Range(0, 60)] [SerializeField] private float rotationSpeed = 4f;

    [Header("Flip the GunHolder")]
    private bool _isFacingRight = true;

    #endregion

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


    // Start is called before the first frame update
    void Start()
    {
        GunStartingPos = GunPivot.localScale;
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        if(!grappleGunConfig.isActiveAndEnabled)
        {
            grappleGunConfig._springJoint.enabled = false;
        }
    }

    public void ObservedUpdate()
    {
        GunPivot.transform.position = Player.transform.position;

        //Checking whether the player body is active and if not we are deactivating this gun pivot
        DestroyIfThePlayerBodyisNull();
    }

    #region Gun Aiming Configs (Look Configs)

    private bool IsFacingRight() => _isFacingRight;

    public void GunAim(Vector3 lookPoint, bool allowRotationOverTime)
    {
        Vector3 PlayerArmDirection = lookPoint - GunPivot.position;

        //Calculating Angle in Degree
        float angle = Mathf.Atan2(PlayerArmDirection.y, PlayerArmDirection.x) * Mathf.Rad2Deg;

        if (!IsFacingRight())
        {
            angle += 180f;
        }

        Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);

        if (rotateOverTime && allowRotationOverTime)
        {
            GunPivot.rotation = Quaternion.Lerp(GunPivot.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }
        else
        {
            GunPivot.rotation = targetRot;
        }
    }

    private void GunFacing(Vector3 lookPoint)
    {
        Vector3 toCursor = lookPoint - _gunHolder.position;
        _isFacingRight = toCursor.x >= 0f;

        Vector3 scale = _gunHolder.localScale;
        scale.x = Mathf.Abs(scale.x) * (_isFacingRight ? 1f : -1f);
        _gunHolder.localScale = scale;
    }

    public void GunAim_with_CursorUI_To_World_Conversion()
    {

        //Converting cursor UI position to screen point
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, UserInputs.instance._cursorTransform.position);

        //Convert screen point to world point; set z for gun'z aiming plane
        float z = Mathf.Abs(GunPivot.position.z - Camera.main.transform.position.z);
        Vector3 cursorWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, z));
        _cursorWorldPos = cursorWorldPos;

        if(cursorWorldTarget != null)
        {
            cursorWorldTarget.position = cursorWorldPos;
        }

        //Here we are gonna set the cursor Limit
        Vector2 playerToCursor = (cursorWorldPos - GunPivot.position).normalized;
        Vector2 cursorVector = playerToCursor * limitRadius;
        Vector2 finalcursorPos = (Vector2)GunPivot.position + cursorVector;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 playerToMouse = (mousePosition - (Vector2)GunPivot.position).normalized;
        Vector2 finalMousePos = (Vector2)GunPivot.position + playerToMouse * limitRadius;

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

        GunFacing(cursorWorldPos);
        GunAim(cursorWorldPos, true);
    }
    #endregion


    void DestroyIfThePlayerBodyisNull()
    {
        if(!player.activeInHierarchy)
        {
            gameObject.SetActive(false);
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(GunPivot.position, limitRadius);
    }

    private void OnDisable()
    {
        UpdateManager.UnregisterObserver(this);
    }
}
