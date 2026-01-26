using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetIndicaotrManager : MonoBehaviour, IUpdateObserver
{
    //instance
    public static TargetIndicaotrManager _instance;

    [SerializeField]private GameObject[] _targets;
    [SerializeField] private GameObject _enemyIndicator;
    [SerializeField] private GameObject _missileIndicator;
    private bool _wasWaveActive;

    private SpriteRenderer _enemyTargetIndicatorSpirte;
    private SpriteRenderer _missileTargetIndicatorSpirte;
    private float _spriteWidth;
    private float _spriteHeight;

    //Guard flag for creating the indicator
    private bool _lastEnemiesIndicatorsAdded = false;

    private Camera _camera;

    //Dictionary For Enemies
    [SerializeField] private Dictionary<GameObject, GameObject> _targetIndicators = new Dictionary<GameObject, GameObject>();

    //Dictionary for Missiles
    [SerializeField] private Dictionary<GameObject, List<GameObject>> _tracerActiveMissiles = new Dictionary<GameObject, List<GameObject>>();

    private void OnEnable()
    {
        UpdateManager.RegisterObserver(this);
    }

    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;   
    }

    private void Start()
    {
        _camera = Camera.main;
        _enemyTargetIndicatorSpirte = _enemyIndicator.GetComponent<SpriteRenderer>();   
        _missileTargetIndicatorSpirte = _missileIndicator.GetComponent<SpriteRenderer>();

        //Enemy Indicator sprite
        var enemySpriteBounds = _enemyTargetIndicatorSpirte.bounds;
        _spriteHeight = enemySpriteBounds.size.y / 2f;
        _spriteWidth = enemySpriteBounds.size.x / 2f;

        //Missile Indicator Sprite
        var missileSpriteBounds = _missileTargetIndicatorSpirte.bounds;
        _spriteHeight = missileSpriteBounds.size.y / 2f;
        _spriteWidth = missileSpriteBounds.size.x / 2f;
    }

    #region Target Indicators For Enemy
    private IEnumerator AddTargetToTheDictionary()
    {
       while(GameManager._instance._waveActive)
       {
            CleanUpDeadTargetsInTargetIndicators();

            if (!_lastEnemiesIndicatorsAdded && GameManager._instance.IfRemainingEnemies()) //Taking the _enemiesAlive from the GameManager to get the last two enemy
            {
                foreach (var enemyTarget in GameManager._instance._enemiesGeneralList)
                {
                    if(_targetIndicators.ContainsKey(enemyTarget) || !enemyTarget.activeInHierarchy || enemyTarget == null)
                        continue;

                    var indicator = PoolManager.SpawnObject(_enemyIndicator, _enemyIndicator.transform.position, Quaternion.identity);
                    indicator.SetActive(false);

                    _targetIndicators.Add(enemyTarget, indicator);
                }

              _lastEnemiesIndicatorsAdded = true;
              yield break;
            }

            yield return new WaitForSeconds(0.25f);
       }

       ResetIndicatorForNextWave();
    }

    private void CleanUpDeadTargetsInTargetIndicators()
    {
        var deadTargets = new List<GameObject>();

        foreach(var kvp in _targetIndicators)
        {
            if(kvp.Key == null || !kvp.Key.activeInHierarchy)
            {
                kvp.Value.SetActive(false);
                PoolManager.ReturnObjectToPool(kvp.Value);
                deadTargets.Add(kvp.Key);
            }
        }

        foreach(var dead in deadTargets)
        {
            _targetIndicators.Remove(dead);
        }
    }

    private void ResetIndicatorForNextWave()
    {
        foreach(var kvp in _targetIndicators)
        {
            kvp.Value.SetActive(false);
            PoolManager.ReturnObjectToPool(kvp.Value);
        }

        _targetIndicators.Clear();
        _lastEnemiesIndicatorsAdded = false;
    }

    private void StartIndicatorCoroutine()
    {
        bool isWaveActive = GameManager._instance._waveActive;

        //Waver started
        if(isWaveActive && !_wasWaveActive)
        {
            _lastEnemiesIndicatorsAdded = false;
            StartCoroutine(AddTargetToTheDictionary());
        }

        _wasWaveActive = isWaveActive;
    }

    #endregion

    #region Target Indicators for Missiles

    public void OnTracerMissileSpawned(GameObject tracer, GameObject missile)
    {
        if (tracer == null)
            return;

        if(!_tracerActiveMissiles.ContainsKey(tracer))
        {
            _tracerActiveMissiles[tracer] = new List<GameObject>();
        }

        _tracerActiveMissiles[tracer].Add(missile);

        //First Missile -> creating Indicator
        if (_tracerActiveMissiles[tracer].Count == 1)
        {
            CreateIndicatorForMissile(missile);
        }
    }

    public void OnTracerMissileDestroyed(GameObject tracer, GameObject missile)
    {
        if(tracer == null || !_tracerActiveMissiles.ContainsKey(tracer))
        {
            return;
        }

        _tracerActiveMissiles[tracer].Remove(missile);

        //Last missile gone -> remove Indicator
        if (_tracerActiveMissiles[tracer].Count <= 0)
        {
            _tracerActiveMissiles.Remove(tracer);
            RemoveIndicatorForMissile(missile);
        }
    }

    private void CreateIndicatorForMissile(GameObject missile)
    {
        if (_targetIndicators.ContainsKey(missile))
            return;

        var indicator = PoolManager.SpawnObject(_missileIndicator, _missileIndicator.transform.position, Quaternion.identity);

        indicator.SetActive(false);
        _targetIndicators.Add(missile, indicator);
    }

    private void RemoveIndicatorForMissile(GameObject missile)
    {
        if (!_targetIndicators.TryGetValue(missile, out var indicator))
            return;

        indicator.SetActive(false);
        PoolManager.ReturnObjectToPool(indicator);
        _targetIndicators.Remove(missile);

    }

    #endregion

    public void ObservedUpdate()
    {
        StartIndicatorCoroutine();

        foreach (KeyValuePair<GameObject, GameObject> entry in _targetIndicators)
        {
            var target = entry.Key;
            var indicator = entry.Value;

            UpdateTarget(target, indicator);
        }
    }

    private void UpdateTarget(GameObject target, GameObject indicator)
    {
        if(!target.activeInHierarchy)
        {
            indicator.SetActive(false);
            return;
        }

        //Convert target position to ViewPort position and ViewPort space is reolution Independent
        var screenPos = _camera.WorldToViewportPoint(target.transform.position);

        //Checking offScreen if the target offScreen means if any axis outside [0,1] target is OffScreen
        bool isOffscreen = screenPos.x <= 0 || screenPos.x >= 1 || screenPos.y <= 0 || screenPos.y >= 1;

        if(isOffscreen)
        {
            indicator.SetActive(true);
            //Convert sprite size into viewport Space [Converts the sprites half size into viewport units] 
            var spriteSizeInVirePort = _camera.WorldToViewportPoint(new Vector3(_spriteWidth, _spriteHeight, 0)) - _camera.WorldToViewportPoint(Vector3.zero);

            //Setting clamps in each axis to keep the indicator inside the sceen
            screenPos.x = Mathf.Clamp(screenPos.x, spriteSizeInVirePort.x, 1 - spriteSizeInVirePort.x);
            screenPos.y = Mathf.Clamp(screenPos.y, spriteSizeInVirePort.y, 1 - spriteSizeInVirePort.y);

            //Converting clamped viewPort position back into world space
            var worldPos = _camera.ViewportToWorldPoint(screenPos);
            worldPos.z = 0;
            indicator.transform.position = worldPos;

            Vector3 direction = target.transform.position - indicator.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            indicator.transform.rotation = Quaternion.Euler(new Vector3(0,0,angle));

        }
        else
        {
            indicator.SetActive(false);
        }
    }

    private void OnDisable()
    {
        UpdateManager.UnregisterObserver(this);
    }
}
