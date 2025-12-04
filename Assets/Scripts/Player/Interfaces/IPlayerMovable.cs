using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerMovable 
{
    Rigidbody2D RB { get; set; }

    bool IsFacingRight { get; set; }

    void MovePlayer(Vector2 velocity);
}
