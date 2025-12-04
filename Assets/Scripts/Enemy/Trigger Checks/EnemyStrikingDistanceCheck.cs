using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStrikingDistance : MonoBehaviour
{
    public GameObject playerTarget { get; set; }

    private Enemy _enemy;

    private void Awake()
    {
        playerTarget = GameObject.FindGameObjectWithTag("Player");

        _enemy = GetComponentInParent<Enemy>();
    }


   
}
