using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShakeController : MonoBehaviour
{
    [Header("Camera Bump")]
    [SerializeField] private float _bumpDuration = 0.08f;

    [Header("Camera Shake")]
    [HideInInspector] public float shakeStrength;
    [SerializeField] private float _shakeDecay = 6f;
    [SerializeField] private float _noiseSpeed = 30f;


    public Vector3 currentShakeOffset { get; private set; }


    [Header("Position and noise config")]
    private Vector3 _baseLocalPosition;
    private Vector2 _noiseSeed;

    [Header("Instance")]
    public static CameraShakeController instance;

    private void Awake()
    {
        if(instance != this && instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        _baseLocalPosition = transform.localPosition;
        //noise seed for camera shake
        _noiseSeed = Random.insideUnitCircle * 1000f;
    }

    //CamBumpy
    public void CameraBump(Vector2 direction, float strength)
    {
        StopAllCoroutines();
        StartCoroutine(BumpRoutine(direction.normalized * strength));
    }

    private IEnumerator BumpRoutine(Vector2 offSet)
    {
        float t = 0f;

        while (t < _bumpDuration)
        {
            t += Time.deltaTime;
            float k = 1f - (t / _bumpDuration);

            Vector3 _offset = (Vector3)(offSet * k);
            currentShakeOffset = _offset;

            transform.localPosition = _baseLocalPosition + (Vector3)(offSet * k);
            yield return null;
        }

        currentShakeOffset = Vector3.zero;
    }


    //Currently I am not Using this functionality because of the parallax effect are being applied to the background so the camera shake works correct but the background images being affected more than the platform and player, for now im just skipping this part, as i am using the gameplay punch method which is nothing but applying force in opposite direction on gameObject which has player, platform and other surrounding as its child, it gives similar effect which was i satisfied with...
    private void CameraShakeUsingPerlinNoise()
    {
        Vector3 shakeOffset = Vector3.zero;

        //cameraShakesXY
        if (shakeStrength > 0f)
        {
            float x = Mathf.PerlinNoise(_noiseSeed.x, Time.time * _noiseSpeed) - 0.5f;
            float y = Mathf.PerlinNoise(_noiseSeed.y, Time.time * _noiseSpeed) - 0.5f;

            shakeOffset = new Vector3(x, y, 0f) * shakeStrength;
            shakeStrength = Mathf.Lerp(shakeStrength, 0f, Time.deltaTime * _shakeDecay);
        }

        currentShakeOffset = shakeOffset;
        transform.localPosition = _baseLocalPosition + shakeOffset;
    }
}
