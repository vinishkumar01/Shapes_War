using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraLookController : MonoBehaviour, ILateUpdateObserver
{
    [Header("LookAhead Settings")]
    [SerializeField] float _horizontalLookAhead = 0.2f; // 0.2 = 20% of screen
    [SerializeField] float _verticalLookAhead = 0.15f; // max down pan
    [SerializeField] float _downAngleThreshold = -30f; // Start panning when aiming below this
    [SerializeField] float _upAngleThreshold = 30f; // Start panning when aiming below this
    [SerializeField] float _horizontalLerpSpeed = 2;
    [SerializeField] float _verticalLerpSpeed = 5f;

    [Header("Player and Cursor")]
    [SerializeField] Transform _player;
    [SerializeField] Animator _playerAnimator;
    [SerializeField] Transform _cursorWorldTarget;

    bool _facingRight = true;

    CinemachineFramingTransposer _currentFraming;

    public void SetFacing(bool isFacingRight)
    {
        _facingRight = isFacingRight;
    }

    private void OnEnable()
    {
        //Register in LateUpdateManager
        LateUpdateManager.RegisterObserver(this);
    }

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        _playerAnimator = GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();

        var vCams = FindObjectsOfType<CinemachineVirtualCamera>();

        foreach(var vCam in vCams)
        {
            vCam.Follow = _player.transform;
        }

        var SDCams = FindObjectsOfType<CinemachineStateDrivenCamera>();
        
        foreach(var SDCam in SDCams)
        {
            SDCam.m_AnimatedTarget = _playerAnimator;
        }
    }

    public void ObservedLateUpdate()
    {
        UpdateCurrentFraming();

        if(_currentFraming == null || _player == null || _cursorWorldTarget == null)
            return;

        float centerX = 0.5f;
        float centerY = 0.5f;

        //Horizontal look-Ahead
        float targetX = centerX + (_facingRight ? -_horizontalLookAhead : _horizontalLookAhead);
        _currentFraming.m_ScreenX = Mathf.Lerp(_currentFraming.m_ScreenX, targetX, Time.deltaTime * _horizontalLerpSpeed);

        //Vertical pan based on aim
        Vector3 toCursor = (_cursorWorldTarget.position - _player.transform.position).normalized;

        //convert to a signed angle relative to "forward" where player facing
        float localX = _facingRight ? toCursor.x : -toCursor.x; // flip X when facing left
        float verticalAngle = Mathf.Atan2(toCursor.y, localX) * Mathf.Rad2Deg;

        float targetY = centerY;

        if(verticalAngle < _downAngleThreshold)
        {
            float t = Mathf.InverseLerp(_downAngleThreshold, -90f, verticalAngle);
            targetY = centerY - _verticalLookAhead * t;
        }
        else if(verticalAngle > _upAngleThreshold)
        {
            float t = Mathf.InverseLerp(_upAngleThreshold, 90f, verticalAngle);
            targetY = centerY + _verticalLookAhead * t;
        }

        _currentFraming.m_ScreenY = Mathf.Lerp(_currentFraming.m_ScreenY, targetY, Time.deltaTime * _verticalLerpSpeed);
    }

    private void UpdateCurrentFraming()
    {
        //Get the main brain's active virtual Camera
        var brain = CinemachineCore.Instance.GetActiveBrain(0);
        if (brain == null)
            return;

        var virtualCameraBase = brain.ActiveVirtualCamera as CinemachineVirtualCameraBase;
        if (virtualCameraBase == null) return;

        CinemachineVirtualCamera virtualCamera = null;

        //if the active virtualCamera is a stateDrivenCamera, get its live child
        var stateDriven = virtualCameraBase as CinemachineStateDrivenCamera;
        if(stateDriven != null)
        {
            var child = stateDriven.LiveChild as CinemachineVirtualCameraBase;
            virtualCamera = child as CinemachineVirtualCamera;
        }
        else
        {
            //otherwise try to use the active virtual camera directly
            virtualCamera = virtualCameraBase as CinemachineVirtualCamera;
        }

        if (virtualCamera == null) return;

        _currentFraming = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();

    }

    private void OnDisable()
    {
        //Register in LateUpdateManager
        LateUpdateManager.UnregisterObserver(this);
    }
}
