using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCameraFollow_Player : MonoBehaviour, ILateUpdateObserver
{
    [Header("Player Transform")]
    [SerializeField] private Transform _player;

    [Header("MiniMap camera Transform")]
    [SerializeField] private float _distance = -5f;

    private void OnEnable()
    {
        LateUpdateManager.RegisterObserver(this);
    }

    public void ObservedLateUpdate()
    {
        Vector3 pos = _player.position;

        pos.z = _distance;

        transform.position = pos;
    }

    private void OnDisable()
    {
        LateUpdateManager.UnregisterObserver(this);
    }
}
