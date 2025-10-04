using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class homingMissile : MonoBehaviour
{
    [SerializeField] Transform player;

    Rigidbody2D rb;

    [SerializeField] int MissileSpeed = 5;
    [SerializeField] float rotateSpeed = 200f;



    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); 
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 direction = ((Vector2)player.position - rb.position).normalized;

        float rotateAmount = Vector3.Cross(direction, transform.up).z;

        rb.angularVelocity = -rotateAmount * rotateSpeed;

        rb.velocity = transform.up * MissileSpeed;
    }
}
