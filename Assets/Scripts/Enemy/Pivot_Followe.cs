using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pivot_Followe : MonoBehaviour
{
    [SerializeField] Transform target;

    private void FixedUpdate()
    {
        if (target != null)
        {
            Vector2 Offset = new Vector2(1.1f, -2.20f);
            Vector2 newPos = (Vector2) target.position + Offset;

            //Move parent Smoothly using RigidBody movement 
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if(rb != null)
            {
                rb.MovePosition(newPos);
            }
            else
            {
                transform.position = newPos; //fallback
            }
        }
    }
}
