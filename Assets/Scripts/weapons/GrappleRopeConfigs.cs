using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleRopeConfigs : MonoBehaviour, IUpdateObserver
{
    [Header("General References:")]
     [SerializeField]private GrappleGun grapplingGun;
    [SerializeField]private LineRenderer _lineRenderer;

    [Header("General Settings")]
    [SerializeField]private int precision = 40; // precision is nothing but the perfection of the rope, not only for perfection this will be assigned to positionCount of the line Renderer where we will be drawing the line with 40 points so that we can get the curve animation
    [Range(0, 20)][SerializeField]private float straightenLineSpeed = 5;

    [Header("Rope Animation Settings")]
    public AnimationCurve ropeAnimationCurve;
    [Range(0.01f, 4)][SerializeField]private float StartWaveSize = 2;
    private float WaveSize = 0;

    [Header("Rope Progression:")]
    public AnimationCurve ropeProgressionCurve;
    [Range(1, 50)][SerializeField]private float ropeProgressionSpeed = 1;
    
    private float moveTime = 0;
    public bool isgrappling = true;
    public bool StraightLine = true;

    [Header("Rope SFX")]
    [SerializeField] private AudioClip _ropeTravel;
    [SerializeField] private AudioClip _hookImpact;
    private bool _impactPlayed = false;

    private void OnEnable()
    {
        //Register in UpdateManager
        UpdateManager.RegisterObserver(this);

        moveTime = 0;
        WaveSize = StartWaveSize;
        StraightLine = false;
        isgrappling = false;

        _impactPlayed = false;

        _lineRenderer.positionCount = precision;
        linePointsToFirePoint();
        _lineRenderer.enabled = true;

        //Rope Travel SFX
        SFXManager._instance.playSFX(_ropeTravel, gameObject.transform.position, 0.5f,false, false);
    }

    private void Awake()
    {
        grapplingGun = GetComponentInParent<GrappleGun>();
        _lineRenderer.enabled = false;
    }

    private void OnDisable()
    {
        //UnRegister in UpdateManager
        UpdateManager.UnregisterObserver(this);

        _lineRenderer.enabled = false;
        isgrappling = false;
    }

    
    void linePointsToFirePoint()
    {
        for(int i = 0; i < precision; i++)
        {
            _lineRenderer.SetPosition(i, grapplingGun._firePoint.position);
        }
    }


    public void ObservedUpdate()
    {
        moveTime += Time.deltaTime;
        DrawRope();
    }

    void DrawRope()
    {
        if (_lineRenderer.positionCount != precision)
        {
            _lineRenderer.positionCount = precision;
        }
            
        Vector2 lineEnd = _lineRenderer.GetPosition(precision - 1);
        Vector2 targetpoint = grapplingGun._grapplePoint;

        if (!StraightLine)
        {
            //Debug.Log("still not a straight line");
            // We are checking that if the linerenderer(line) has reached to grapple point, if reached then the line becomes straight, until it reaches, the line will be drawn curvy
            
            if (Vector2.Distance(lineEnd, targetpoint) <= 0.05f)
            {   
                //Debug.Log(" straight line");
                StraightLine = true;

                //We will play the hook impact sound here
                if(!_impactPlayed)
                {
                    _impactPlayed = true;

                    SFXManager._instance.playSFX(_hookImpact, grapplingGun._grapplePoint, 1f, false, false);
                }
            }
            else
            {
                DrawRopeWaves();
            }
        }
        else
        {
            if(!isgrappling)
            {
                grapplingGun.GrappleConfigs();
                isgrappling = true;
            }


            if (WaveSize > 0)
            {
                // We are checking if the Wave Size greater than zero if yes then we are making the curvy line to straight line as time passes and multiply with straightlinespeed (how fast the line has to become straight)
                WaveSize -= Time.deltaTime * straightenLineSpeed;
                DrawRopeWaves();
            }
            else
            {
                WaveSize = 0;
                if(_lineRenderer.positionCount != 2)
                {
                    _lineRenderer.positionCount = 2;
                }
                DrawRopeNoWaves();
            }
        }
    }

    void DrawRopeWaves()
    {
        for(int i = 0; i < precision; i++)
        {
            //delta varaible determines what part of the distance from firePoint to grapple point at the point should pass 
            float delta = (float)i / ((float)precision - 1f);
            Vector2 offset = Vector2.Perpendicular(grapplingGun._grappleDistanceVector).normalized * ropeAnimationCurve.Evaluate(delta) * WaveSize;
            Vector2 targetPosition = Vector2.Lerp(grapplingGun._firePoint.position, grapplingGun._grapplePoint, delta) + offset;
            Vector2 currentPosition = Vector2.Lerp(grapplingGun._firePoint.position, targetPosition, ropeProgressionCurve.Evaluate(moveTime) * ropeProgressionSpeed);

            _lineRenderer.SetPosition(i, currentPosition);
        }
    }

    void DrawRopeNoWaves()
    {
        _lineRenderer.SetPosition(0, grapplingGun._firePoint.position);
        _lineRenderer.SetPosition(1, grapplingGun._grapplePoint);
    }

}
