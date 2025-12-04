using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/SO's/PlayerDataSO")]
public class PlayerDataSO : ScriptableObject
{
    [Header("Player Health Attributes")]
    public int maxHealth = 200;
    public int damageAmount =  20;

    [Header("Surrounding and Rope Check")]
    public float groundCheckRadius = 0.5f;
    public float ropeCheckRadius = 2f;

    [Header("Rope Configs")]
    public float reattachDelay = 0.5f;

    [Header("Gravity Configs")]
    public float fallGravityMult = 1.5f; //Multiplier to the player's gravityScale when falling.
    public float maxFallSpeed = 25f; //Maximum fall speed (terminal velocity) of the player when falling. clamping it
    public float fastFallGravityMult = 2f; //Larger multiplier to the player's gravityScale when they are falling and a downwards input is pressed.
                                                 //Seen in games such as Celeste, lets the player fall extra fast if they wish.
    public float maxFastFallSpeed = 30f; //Maximum fall speed(terminal velocity) of the player when performing a faster fall. 

    [Header("Jump configs")]
    public float jumpHeight = 3f;
    public float jumpTimeToApex = 0.2f;

    [Header("Mario Jump Effect")]
    public float jumpTime = 0.25f;

    [Header("Coyote Time")]
    public float coyoteTime = 0.5f;

    [Header("Jump Buffer")]
    public float jumpBufferTime = 0.5f;

    [Header("Player Move State Variables")]
    [Header("Movement Configs")]
    public float movementSpeed = 16f;
    public float accelerationTimeAirBorne = 0.2f;
    public float accelerationTimeGround = 0.1f;
    public float ropeSwingForce = 8f;

    [Header("Player Jump State Variables")]
    [Header("Double Jump")]
    public float doubleJumpForce = 25f;
    public bool doubleJumpSkill = false;

    [Header("Move in Air")]
    public float airMovementSpeed = 16f;
    public float accelerationTimeAir = 0.15f;
    public float ropeSwingForceInJump = 8f;

    [Header("Player Dash")]
    public float dashTime = 0.2f;
    public float dashSpeed = 50f;
    public float dashCoolDown = 2.5f;

}
