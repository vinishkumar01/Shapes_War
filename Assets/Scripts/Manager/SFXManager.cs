using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SFXManager : MonoBehaviour
{
   public static SFXManager _instance;

    [SerializeField] private AudioSource _sfxSoundObject;
    private Transform _cameraTransform;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        _cameraTransform = Camera.main.transform;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Camera cam = Camera.main;

        if(cam != null)
        {
            _cameraTransform = cam.transform;
        }
        else
        {
            _cameraTransform = null;
        }
    }

    private bool isInsideCameraView(Vector3 WorldPos)
    {
        Camera cam = Camera.main;
        if(cam == null) return false;

        Vector3 viewPortPos = cam.WorldToViewportPoint(WorldPos);

        return viewPortPos.x >= 0 && viewPortPos.x <= 1 && viewPortPos.y >= 0 && viewPortPos.y <= 1;
    }


    private void Apply2DSpatial(AudioSource audioSource, Vector3 worldPos, float baseVolume)
    {
        if (_cameraTransform == null)
        {
            Camera cam = Camera.main;
            if(cam != null)
            {
                _cameraTransform = cam.transform;
            }
            else
            {
                return;
            }
        }

        float dx = worldPos.x - _cameraTransform.position.x;
        float distance = Mathf.Abs(dx);

        float maxDistance = 10f;

        //volume falloff
        float volume = 1f - Mathf.Clamp01(distance / maxDistance);

        if (isInsideCameraView(worldPos))
        {
            volume = Mathf.Max(volume, 0.4f);
            audioSource.panStereo = 0f;
        }

        volume = Mathf.SmoothStep(0f, 1f, volume);

        //Stereo pan 
        float pan = Mathf.Clamp(dx / maxDistance, -1f, 1f);

        audioSource.volume = volume * baseVolume;
        audioSource.panStereo = pan;
    }

    public AudioSource playSFX(AudioClip clip, Vector3 pos, float volume, bool applySpatialEffect, bool loop = false)
    {
        AudioSource audioSource = PoolManager.SpawnObject(_sfxSoundObject, pos, Quaternion.identity, PoolManager.PoolType.SoundFX);

        //restting 
        audioSource.Stop();

        audioSource.clip = clip;
        audioSource.loop = loop;

        audioSource.pitch = Random.Range(0.95f, 1.05f);

        //applying spatial logic
        if(applySpatialEffect)
        {
            Apply2DSpatial(audioSource, pos, volume);
        }
        else
        {
            audioSource.volume = volume;
            audioSource.panStereo = 0f;
        }

        audioSource.Play();

        if(!loop)
        {
            StartCoroutine(ReturnAfterPlay(audioSource, clip.length));
        }

        return audioSource;
    }

    private IEnumerator ReturnAfterPlay(AudioSource audioSource, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (audioSource != null)
        {
            PoolManager.ReturnObjectToPool(audioSource.gameObject, PoolManager.PoolType.SoundFX);
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
