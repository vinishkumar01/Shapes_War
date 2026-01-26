using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Player_Camera_Bind : MonoBehaviour
{
    [SerializeField] Animator Playeranimator;
    // Start is called before the first frame update
    void Start()
    {
        Playeranimator = GetComponent<Animator>();

        var vCams = FindObjectsOfType<CinemachineVirtualCamera>();
        foreach(var vCam in vCams)
        {
            vCam.Follow = transform;
        }

        var SDCam = FindObjectOfType<CinemachineStateDrivenCamera>();

        SDCam.m_AnimatedTarget = Playeranimator;
    }

    
}
