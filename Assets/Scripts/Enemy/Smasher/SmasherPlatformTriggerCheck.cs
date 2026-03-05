using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmasherPlatformTriggerCheck : MonoBehaviour
{
    [SerializeField] private LayerMask _platformLayer;
    [SerializeField] private Smasher _smasher;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((_platformLayer & (1 << collision.gameObject.layer)) != 0)
        {
            Debug.Log("Collided with the platform");
            _smasher.FlipToAvoidEdges();
        }
    }
}
