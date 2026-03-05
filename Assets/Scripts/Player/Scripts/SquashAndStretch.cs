using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquashAndStretch : MonoBehaviour
{
    Vector3 _currentScale;

    Vector3 _scaleVelocity;

    [Header("Settings")]
    [SerializeField] private float _returnTime = 0.05f;

    

    private void Awake()
    {
        _currentScale = transform.localScale;
    }

    private void LateUpdate()
    {
        //Fast snap back (springy)

        _currentScale = Vector3.SmoothDamp(_currentScale, transform.localScale, ref _scaleVelocity, _returnTime);

        transform.localScale = _currentScale;
    }


    public void Squash(float xAmount, float yAmount)
    {
        //Instant deformation
        _currentScale += new Vector3(xAmount, yAmount, 0f);
    }

    public void MicroJitter()
    {
        transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-1f, 1f));
    }

 

}
