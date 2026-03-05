using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunRecoil : MonoBehaviour, ILateUpdateObserver
{
    private Vector2 _offset;
    private Vector2 _offsetVelocity;

    [Header("Settings")]
    [SerializeField] private float _recoilDistance = 0.12f;
    [SerializeField] private float _returnTime = 0.04f;

    private void OnEnable()
    {
        LateUpdateManager.RegisterObserver(this);
    }

    public void ObservedLateUpdate()
    {
        _offset = Vector2.SmoothDamp(_offset, Vector2.zero, ref _offsetVelocity, _returnTime);

        transform.localPosition = _offset;
    }

    public void RecoilKick(Vector2 fireDirection)
    {
        _offset += -fireDirection.normalized * _recoilDistance;
    }

    private void OnDisable()
    {
        LateUpdateManager.UnregisterObserver(this);
    }

    
}
