using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpritesFollowplayer : MonoBehaviour, ILateUpdateObserver
{
    [Header("Player Position")]
    [SerializeField] private GameObject _player;
    private Player _playerScript;
    private Vector3 _localOffset;

    [Header("Ground Snap")]
    [SerializeField] private float _groundedYOffset = -0.1f;
    [SerializeField] private float _airYOffset = 0f;

    private void OnEnable()
    {
        LateUpdateManager.RegisterObserver(this);
    }

    private void Awake()
    {
        _playerScript = _player.GetComponent<Player>();
        _localOffset = transform.localPosition;
    }

    public void ObservedLateUpdate()
    {
        PlayerVisualGroundSnap();

       gameObject.transform.position = _player.transform.position + _localOffset;

        if(!_player.activeInHierarchy)
        {
            gameObject.SetActive(false);
        }
    }

    private void PlayerVisualGroundSnap()
    {
        bool grounded = _playerScript._isGrounded;

        Vector3 pos = transform.localPosition;
        pos.y = grounded ? _groundedYOffset : _airYOffset;
        transform.localPosition = pos;
    }

    private void OnDisable()
    {
        LateUpdateManager.UnregisterObserver(this);
    }
}
