using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFollowTracer : MonoBehaviour, ILateUpdateObserver
{
    [Header("Enemy Position")]
    [SerializeField] private GameObject _tracer;
     private Tracer _tracerScript;
    private Vector3 _localOffset;

    [Header("Ground Snap")]
    [SerializeField] private float _groundedYOffset = -0.1f;
    [SerializeField] private float _airYOffset = 0f;


    private void OnEnable()
    {
        _tracerScript = _tracer.GetComponent<Tracer>();

        LateUpdateManager.RegisterObserver(this);
    }

    private void Awake()
    {
        _localOffset = transform.localPosition;
    }

    public void ObservedLateUpdate()
    {
        EnemyVisualGroundSnap();

        gameObject.transform.position = _tracer.transform.position + _localOffset;

        gameObject.transform.rotation = Quaternion.identity;

        if(!_tracer.activeInHierarchy)
        {
            gameObject.SetActive(false);
        }

    }

    private void EnemyVisualGroundSnap()
    {
        bool grounded = _tracerScript.isGrounded;  

        Vector3 pos = transform.localPosition;
        pos.y = grounded ? _groundedYOffset : _airYOffset;
        transform.localPosition = pos;
    }

    private void OnDisable()
    {
        LateUpdateManager.UnregisterObserver(this);
    }
}
