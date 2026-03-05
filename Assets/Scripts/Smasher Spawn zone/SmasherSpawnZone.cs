using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmasherSpawnZone : MonoBehaviour
{
    private BoxCollider2D _boxCollider;

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
    }

    public bool containsNode(Vector2 position)
    {
        Debug.Log("Bounds Information"+_boxCollider.bounds);
        return _boxCollider.bounds.Contains(position);
    }
}
