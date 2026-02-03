using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ParallexManager : MonoBehaviour, ILateUpdateObserver
{
    [SerializeField] private Transform[] _backGroundImages;
    [SerializeField] private float[] _parallaxScale; // The proportion of the player movement to move the background 

    [SerializeField] private float _zoomParallaxStrength = 0.5f; //How much zoom affects the parallax

    private Transform _cam;
    private CinemachineBrain _brain;

    private Vector3 _previousCamPos;
    private float _previousOrthoSize;

    [Header("Camera Shake Controller")]
    [SerializeField] private CameraShakeController _cameraShake;

    private void Awake()
    {
        _cam = Camera.main.transform;
        _brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    private void OnEnable()
    {
        _previousCamPos = _cam.position;

        if(_brain != null && _brain.ActiveVirtualCamera != null)
        {
            _previousOrthoSize = _brain.ActiveVirtualCamera.State.Lens.OrthographicSize;
        }

        _parallaxScale = new float[_backGroundImages.Length];

        for(int i =0; i < _backGroundImages.Length; i++)
        {
            _parallaxScale[i] = 1f / (i + 2f);
        }

        LateUpdateManager.RegisterObserver(this);
    }

    public void ObservedLateUpdate()
    {

        if(_cam == null || _brain == null) return;
        if(_brain.ActiveVirtualCamera == null) return;

        Vector3 shakeOffset = _cameraShake != null ? _cameraShake.currentShakeOffset : Vector3.zero;

        //Remove Shake influence from camera position
        Vector3 stableCameraPos = _cam.position - shakeOffset;

        float camDeltaX = stableCameraPos.x - _previousCamPos.x;

        float currentOrthoSize = _brain.ActiveVirtualCamera.State.Lens.OrthographicSize;

        float zoomDelta = currentOrthoSize - _previousOrthoSize;

        for(int i =0; i < _backGroundImages.Length; i++)
        {
            //position based parallax
            float posParallax = camDeltaX * _parallaxScale[i];

            //Zoom based parallax
            float zoomParallax = -zoomDelta * _zoomParallaxStrength * _parallaxScale[i];

            float targetX = _backGroundImages[i].position.x + posParallax + zoomParallax;

            Vector3 targetPos = new Vector3(targetX, _backGroundImages[i].position.y, _backGroundImages[i].position.z);

            //_backGroundImages[i].position = Vector3.Lerp(_backGroundImages[i].position, targetPos, Smoothing * Time.deltaTime);

            _backGroundImages[i].position = targetPos;  
        }

        _previousCamPos = stableCameraPos;
        _previousOrthoSize = currentOrthoSize;

    }

    //Just in case if we are changing the vCamera state manually we can call this method 
    public void OnCameraStateChanged()
    {
        _previousCamPos = _cam.position;

        if(_brain.ActiveVirtualCamera != null)
        {
            _previousOrthoSize = _brain.ActiveVirtualCamera.State.Lens.OrthographicSize;
        }
    }

    private void OnDisable()
    {
        LateUpdateManager.UnregisterObserver(this);
    }
}
