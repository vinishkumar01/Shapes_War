using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private Player _player;

    private void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        _player = player?.GetComponent<Player>();
    }

    private void OnPlayerIdle()
    {
        _player.AnimationTriggerEvent(Player.AnimationTriggerType.Idle);
    }

    private void OnPlayerMove()
    {
        _player.AnimationTriggerEvent(Player.AnimationTriggerType.Run);
    }

    private void OnPlayerJump()
    {
        _player.AnimationTriggerEvent(Player.AnimationTriggerType.Jump);
    }

    private void OnPlayerDash()
    {
        _player.AnimationTriggerEvent(Player.AnimationTriggerType.Dash);
    }
}
