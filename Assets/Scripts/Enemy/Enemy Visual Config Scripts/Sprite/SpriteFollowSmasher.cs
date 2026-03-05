using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFollowSmasher : MonoBehaviour, ILateUpdateObserver
{
    [Header("Enemy Position")]
    [SerializeField] private GameObject _smasher;
     private Smasher _smasherScript;
    private Vector3 _localOffset;

    [Header("Ground Snap")]
    [SerializeField] private float _groundedYOffset = -0.1f;
    [SerializeField] private float _airYOffset = 0f;


    private void OnEnable()
    {
        _smasherScript = _smasher.GetComponent<Smasher>();

        LateUpdateManager.RegisterObserver(this);
    }

    private void Awake()
    {
        _localOffset = transform.localPosition;
    }

    public void ObservedLateUpdate()
    {
        EnemyVisualGroundSnap();

        gameObject.transform.position = _smasher.transform.position + _localOffset;

        //gameObject.transform.rotation = Quaternion.identity;

        if(!_smasher.activeInHierarchy)
        {
            gameObject.SetActive(false);
        }

    }

    private void EnemyVisualGroundSnap()
    {
        bool grounded = _smasherScript.isGrounded;  

        Vector3 pos = transform.localPosition;
        pos.y = grounded ? _groundedYOffset : _airYOffset;
        transform.localPosition = pos;
    }

    private void OnDisable()
    {
        LateUpdateManager.UnregisterObserver(this);
    }
}
