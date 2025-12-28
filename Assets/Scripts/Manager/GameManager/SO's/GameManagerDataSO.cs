using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameManager/SO's/GameManagerDataSO")]
public class GameManagerDataSO : ScriptableObject
{
    [Header("Enemies")]
    public GameObject chaser;
    public GameObject smasher;
    public GameObject tracer;

    [Header("Player")]
    public GameObject player;

    [Header("Power Ups")]
    public GameObject doubleJump;
    public GameObject dash;
    public GameObject grappleAmmo;
    public GameObject healthPack;
    public GameObject ammo;

    [Header("HighScore")]
    public int highScore;
}
