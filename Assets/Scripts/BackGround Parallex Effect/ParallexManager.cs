using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ParallexManager : MonoBehaviour, IUpdateObserver
{
    [SerializeField] private Transform[] _backGroundImages;
    [SerializeField] private float[] _parallaxScale; // The proportion of the player movement to move the background 
    [SerializeField] private float Smoothing = 1f;

    [SerializeField] private float _zoomParallaxStrength = 0.5f; //How much zoom affects the parallax

    private Transform _cam;
    private CinemachineBrain _brain;

    private Vector3 _previousCamPos;
    private float _previousOrthoSize;

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
            _parallaxScale[i] = 0.1f * (i + 1);
        }

        UpdateManager.RegisterObserver(this);

        //GameObject player = GameObject.FindGameObjectWithTag("Player");
        //_player = player?.GetComponent<Transform>();
        //if(_player == null)
        //{
        //    Debug.LogWarning("Player Not Found");
        //}
        //else
        //{
        //    _previousPlayerPos = _player.position;
        //}

        //if(_parallaxScale == null || _parallaxScale.Length != _backGroundImages.Length)
        //{
        //    Debug.LogError("ParallaxManager: Parallax Scale array must match backGround images length");
        //}
    }

    public void ObservedUpdate()
    {

        if(_cam == null || _brain == null) return;
        if(_brain.ActiveVirtualCamera == null) return;

        Vector3 camPos = _cam.position;

        float camDeltaX = camPos.x - _previousCamPos.x;

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

            _backGroundImages[i].position = Vector3.Lerp(_backGroundImages[i].position, targetPos, Smoothing * Time.deltaTime);
        }

        _previousCamPos = camPos;
        _previousOrthoSize = currentOrthoSize;

        //if (_player == null) return;

        //Vector3 playerDelta = _player.position - _previousPlayerPos;

        //for(int i = 0; i < _backGroundImages.Length; i++)
        //{
        //    float targetX = _backGroundImages[i].position.x + playerDelta.x * _parallaxScale[i];

        //    Vector3 targetPos = new Vector3(targetX, _backGroundImages[i].position.y, _backGroundImages[i].position.z);

        //    _backGroundImages[i].position = Vector3.Lerp(_backGroundImages[i].position, targetPos, Smoothing);
        //}

        //_previousPlayerPos = _player.position;

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
        UpdateManager.UnregisterObserver(this);
    }
}
