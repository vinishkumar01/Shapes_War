using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleGun : MonoBehaviour, IUpdateObserver
{
    #region References

    [SerializeField] private GunAiming _gunAiming;
    private Player _player;
    private GrappleRopeConfigs _grappleRopeConfigs;
    [SerializeField] private WeaponSO _grappleAttributes;
    [SerializeField] private GameObject _grapple;
    #endregion

    #region Grapple Gun Attributes

    [Header("Transform References")]
    private Transform _playerTransform;
    [SerializeField] private Transform _gunPivot;
    [SerializeField] public Transform _firePoint;

    [Header("Vector References")]
    [SerializeField] public Vector2 _grapplePoint;
    [SerializeField] public Vector2 _grappleDistanceVector;

    [Header("Physics References")]
    [SerializeField] public SpringJoint2D _springJoint;

    [Header("grapple Condition")]
    private bool _grappled = false;

    #endregion

    private void OnEnable()
    {
        UpdateManager.RegisterObserver(this);
    }

    private void Awake()
    {
        if (this.gameObject.activeSelf)
        {
            Debug.Log("Got the Grapple Rope");
            _grappleRopeConfigs = GetComponentInChildren<GrappleRopeConfigs>();
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        _playerTransform = player?.GetComponent<Transform>();
        _player = player?.GetComponent<Player>();
        if(player == null)
        {
            Debug.LogWarning("Player not Found");
        }
    }

    private void Start()
    {
        _grappleRopeConfigs.enabled = false;
        _springJoint.enabled = false;
    }

    public void ObservedUpdate()
    {
        GetGrappleInputs();
    }

    private void GetGrappleInputs()
    {
        _gunAiming.GunAim_with_CursorUI_To_World_Conversion();

        if(_player._playerDataSO._grappleAmmo > 0)
        {
            if(UserInputs.instance._playerInputs.Player.Fire.WasPressedThisFrame())
            {
                if(SetGrapplePoint())
                {
                    _grapple.SetActive(false);
                    _grappleRopeConfigs.enabled = true;
                }
                
            }
            else if(UserInputs.instance._playerInputs.Player.Fire.IsPressed())
            {
                if(_grappleRopeConfigs.enabled)
                {
                    _gunAiming.GunAim(_grapplePoint, false);
                    _grappled = true;
                    _grapple.SetActive(false);
                }
                else
                {
                    _gunAiming.GunAim_with_CursorUI_To_World_Conversion();
                }
            }
            else if(UserInputs.instance._playerInputs.Player.Fire.WasReleasedThisFrame())
            {
                _grappleRopeConfigs.enabled = false;
                _springJoint.enabled = false;
                _player.RB.gravityScale = _player._gravityScale;

                //the ammo will be depleted when the trigger is released 
                if(_grappled)
                {
                    _player._playerDataSO._grappleAmmo--;
                    _player._grappleAmmoUI.text = _player._playerDataSO._grappleAmmo.ToString();
                }

                _grappled = false;
                _grapple.SetActive(true);
            }
        }
        else
        {
            _gunAiming.GunAim_with_CursorUI_To_World_Conversion();
        }
    }

    private bool SetGrapplePoint()
    {
        // We are drawing rayCast to the Mouse Position from the firePoint when this happens if there is something between these two distance like (interactable layer to grapple) we will store the position maybe 
        Vector2 origin = _firePoint.position;
        Vector2 distanceVector = (Vector2)UserInputs.instance._cursorTransform.position - origin;
        Vector2 direction = distanceVector.normalized;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, _grappleAttributes.maxDistance ,_grappleAttributes.hitLayer);


        if (!hit)
            return false;

        // now we are storing the hit position (means wherever in the layer mask) to the grapplePoint which is nothing but the Vector2(x, y)
        _grapplePoint = hit.point;
        //now we are calculating the Distance from the GunPivot to grapplePoint 
        _grappleDistanceVector = _grapplePoint - (Vector2)_gunPivot.position;
        
        return true;

    }

    public void GrappleConfigs()
    {
        _springJoint.autoConfigureDistance = false;
        _springJoint.connectedBody = null;
        _springJoint.connectedAnchor = _grapplePoint;

        Vector2 distanceVector = _firePoint.position - _playerTransform.position;
        _springJoint.distance = distanceVector.magnitude;
        _springJoint.frequency = _grappleAttributes.launchSpeed;
        _springJoint.enabled = true;
    }


    private void OnDrawGizmos()
    {
        if(_firePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_firePoint.position, _grappleAttributes.maxDistance);
        }
    }

    private void OnDisable()
    {
        UpdateManager.UnregisterObserver(this);
    }
}
