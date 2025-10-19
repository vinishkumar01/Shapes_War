using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smasher_Test_Script : MonoBehaviour
{
    [Header("Smasher Configs")]
    [SerializeField] int playerDetectionDistance;
    [SerializeField] float maintainDistancewithPlayer;
    [SerializeField] float GroundCheckRadius;
    [SerializeField] float platformDetectionDistance;
    [SerializeField] float moveSpeed;
    [SerializeField] int facingDirection;

    [Header("Reference")]
    [SerializeField] GameObject GroundCheck;
    [SerializeField] GameObject platformSBCheck;
    [SerializeField] GameObject playerCheck;
    [SerializeField] Collider2D NPCcollider;
    [SerializeField] LayerMask platformLayer;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] Rigidbody2D rb;

    [Header("Conditions")]
    [SerializeField] bool isGrounded;
    [SerializeField] bool platformBelow;
    [SerializeField] bool platformside;
    [SerializeField] bool isPlayerDetected;
    [SerializeField] bool isplayerDetectedBack;
    [SerializeField] bool isfacingLeft;
    [SerializeField] bool isfacingRight;


    // Start is called before the first frame update
    void Start()
    {
        NPCcollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        facingDirection = 1;
        isfacingRight = true;
    }

    // Update is called once per frame
    void Update()
    {
        DrawRaysAndSpheres();
        MoveAndChase();
        Flip();
        flipWhenPlayerDetected();

    }
    
    void DrawRaysAndSpheres()
    {
        //Grounded 
        isGrounded = Physics2D.OverlapCircle(GroundCheck.transform.position, GroundCheckRadius, platformLayer);

        //PLtform Check
        platformBelow = Physics2D.Raycast(platformSBCheck.transform.position, Vector2.down, platformDetectionDistance, platformLayer);
        platformside = Physics2D.Raycast(platformSBCheck.transform.position, transform.right, platformDetectionDistance, platformLayer);

        //PlayerCheck
        isPlayerDetected = Physics2D.Raycast(playerCheck.transform.position, transform.right, playerDetectionDistance, playerLayer);
        isplayerDetectedBack = Physics2D.Raycast(playerCheck.transform.position, -transform.right, playerDetectionDistance, playerLayer);


        Debug.DrawLine(playerCheck.transform.position, playerCheck.transform.position + transform.right * playerDetectionDistance, isPlayerDetected ? Color.red : Color.yellow);
        Debug.DrawLine(playerCheck.transform.position, playerCheck.transform.position + -transform.right * playerDetectionDistance, isplayerDetectedBack ? Color.red : Color.magenta);
    }

    void MoveAndChase()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(facingDirection * Time.deltaTime * moveSpeed, rb.velocity.y);
        }
    }

    void Flip()
    {
        if (!platformBelow || platformside)
        {
            facingDirection *= -1;
            this.transform.Rotate(0f, 180.0f, 0f);
            isfacingLeft = true;
            isfacingRight = false;
        }
    }

    void flipWhenPlayerDetected()
    {
        if (isPlayerDetected && !isfacingLeft)
        {
            facingDirection *= 1;
            this.transform.Rotate(0f, 0f, 0f);
            isfacingLeft = false;
            isfacingRight = true;
        }

        if(isplayerDetectedBack && isfacingRight)
        {
            facingDirection *= -1;
            this.transform.Rotate(0f, 180.0f, 0f);
            isfacingLeft = true;
            isfacingRight = false;
        }

        if(isplayerDetectedBack && isfacingLeft)
        {
            facingDirection *= 1;
            this.transform.Rotate(0f, 0f, 0f);
            isfacingLeft = false;
            isfacingRight = true;
        }
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
            Gizmos.DrawLine(platformSBCheck.transform.position, platformSBCheck.transform.position + transform.right * platformDetectionDistance);
        }
        else
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(platformSBCheck.transform.position, platformSBCheck.transform.position + transform.right * platformDetectionDistance);
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
}
