using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wave_Impact : MonoBehaviour
{

    [Header("Wave Impact configs")]
    [SerializeField] float force;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            Vector2 direction = collision.gameObject.transform.position - transform.position;

            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(direction * force);
        }
    }
}
