using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingGunConfig : MonoBehaviour
{
    [SerializeField] GunAiming gunAiming;
    [SerializeField] PlayerController playerController;
    [SerializeField] Player player;
    [SerializeField] GrappleRopeConfigs grappleRope;

    #region GrapplingGun Attributes

    [Header("Transform Reference")]
    [SerializeField] Transform Player;
    [SerializeField] Transform GunPivot;
    public Transform firePoint;

    [Header("Vector References")]
    public Vector2 grapplePoint;
    public Vector2 grappleDistanceVector;

    [Header("Physics Reference")]
    [SerializeField] public SpringJoint2D _springJoint2D;

    [Header("Layer Reference")]
    [SerializeField] int grappleLayerNumber = 6;

    [Header("Distance Configs")]
    [SerializeField] float maxDistance = 20f;

    [Header("Auto Configure Distance")]
    //[SerializeField] bool autoConfigureDistance = false;
    //[SerializeField] float targetDistance = 3;
    //[SerializeField] float targetFfrequency = 1;

    [Header("Launch")]
    [SerializeField] float launchSpeed = 1;


   

    #endregion

    private void Awake()
    {
        gunAiming = GetComponentInParent<GunAiming>();
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponentInParent<PlayerController>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        grappleRope = GetComponentInChildren<GrappleRopeConfigs>();
    }

    private void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        grappleRope.enabled = false;
        _springJoint2D.enabled = false;
    }

    #region GrappleGun
    public void GrapplingGunConfigs()
    {
        GrappleGunAimAndSomeStuff();
    }

    void GrappleGunAimAndSomeStuff()
    {
        if (UserInputs.instance._playerInputs.Player.Fire.WasPressedThisFrame())
        {
            SetGrapplingPoint();
            //Debug.Log("Grapple Shot");
        }
        else if (UserInputs.instance._playerInputs.Player.Fire.IsPressed())
        {
            if (grappleRope.enabled)
            {
                gunAiming.GunAim(grapplePoint, false);
                //Debug.Log("Grapple Point is enabled");
            }
            else
            {
                gunAiming.GunAim_with_CursorUI_To_World_Conversion();
            }
        }
        else if (UserInputs.instance._playerInputs.Player.Fire.WasReleasedThisFrame())
        {
            grappleRope.enabled = false;
            _springJoint2D.enabled = false;
            //playerController.rb.gravityScale = playerController.gravityScale;
            player.RB.gravityScale = player._gravityScale;

        }
        else
        {
            gunAiming.GunAim_with_CursorUI_To_World_Conversion();
        }

    }


    void SetGrapplingPoint()
    {
        Vector2 distanceVector = UserInputs.instance._cursorTransform.position - GunPivot.position;

        // We are drawing RayCast to the Mouse position from the firePoint when this happens if there is something between these two distance like (interactable layer to grapple) we will store the position maybe
        Vector2 origin = firePoint.position;
        Vector2 direction = distanceVector.normalized;
        int mask = ~LayerMask.GetMask("BackGround");
        RaycastHit2D hit = Physics2D.Raycast(origin,direction, maxDistance, mask);

        if (hit)
        {
            //Debug.Log("Hit: " + hit.collider.name);
            //Debug.Log("Hittt");
            if (hit.transform.gameObject.layer == grappleLayerNumber)
            {
                //Debug.Log("We hit the platform layer");
                //We are checking that is there any layer within the maxDistance, so lets say that the maxDistance is 20 and within this range the layer should be there

                if (Vector2.Distance(hit.point, firePoint.position) <= maxDistance)
                {
                    // So now we are storing the hit position (means wherever in the layer mask) to the grapplePoint which is nothing but the Vector2(x,y)
                    grapplePoint = hit.point;
                    //here we are calculating the Distance from GunPivot to grapplePoint.
                    grappleDistanceVector = grapplePoint - (Vector2)GunPivot.position;
                    grappleRope.enabled = true;
                }
            }
        }

    }

    public void GrappleConfigs()
    {
        _springJoint2D.autoConfigureDistance = false;
        _springJoint2D.connectedBody = null;
        _springJoint2D.connectedAnchor = grapplePoint;

        Vector2 distanceVector = firePoint.position - Player.position;
        _springJoint2D.distance = distanceVector.magnitude;
        _springJoint2D.frequency = launchSpeed;
        //Debug.Log("Spring frequency: " + _springJoint2D.frequency);
        _springJoint2D.enabled = true;

    }

    private void OnDrawGizmos()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(firePoint.position, maxDistance);
        }
    }

    

    #endregion
}
