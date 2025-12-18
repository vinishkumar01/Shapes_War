using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour, IUpdateObserver
{
    public static GameManager _instance;
    private Transform _player;
    [SerializeField] private List<Node> _allNodesInTheScene = new List<Node>();

    [Header("SO's")]
    [SerializeField] private EnemySpawnListSO _enemySpawnListSO;
    [SerializeField] private EnemiesSO _chaserAttributes;
    [SerializeField] private EnemiesSO _tracerAttributes;
    [SerializeField] private EnemiesSO _smasherAttributes;

    [Header("Enemy Spawn System")]
    [SerializeField] private Dictionary<GameObject, EnemySpawnData> _enemies = new ();
    
    [SerializeField] private float _spawnInterval = 5f;

    [Header("Wave System Attributes")]
    [SerializeField] private int _spawnLimitPerWave = 5;
    [SerializeField] private float _timeBetweenWave = 5f;
    [SerializeField] private float _spawnScalingFactor = 0.1f;
    [SerializeField] private float _partialSpawnScalingFactor = 0.03f;

    [SerializeField] private int _currentWave = 0;
    [SerializeField] private int _enemiesToSpawn;
    [SerializeField] private int _enemiesSpawnedCount;
    [SerializeField] private float timeSinceLastSpawn;
    private int _score;
    [SerializeField] private int _enemiesAlive = 0;
    [SerializeField] private bool _waveActive = false;

    public struct EnemySpawnData
    {
        public float _distanceFromPlayer;
        public int _spawnChances;
        public int _score;

        //Enemy Attributes
        public int _health;
        public int _damageDealAmout;
        public int _damageGives;
        public int _moveSpeed;

        //Tracer
        public int _missileInitiation;
        public float _intervalbetweenMissiles;
        public float _fireRate;

        public float _detectionCheckRadius;
        public float _nearCheckRadius;

        //Smasher
        public int _playerDetectionDistance;
    }

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
        DontDestroyOnLoad(gameObject);


        //Add the Enemies to the list
        if (_enemySpawnListSO != null)
        {

            //Get the data from the SO's 
            _enemies.Add(_enemySpawnListSO.chaser, new EnemySpawnData
            {
                _distanceFromPlayer = 50f,
                _spawnChances = 5,
                _score = 50,
                _health = _chaserAttributes._chaserMaxHealth,
                _damageDealAmout = _chaserAttributes._chaserDamageDealAmount,
                _damageGives = _chaserAttributes._chaserDamageGives,
                _moveSpeed = _chaserAttributes._chaser_Movespeed
            });

            _enemies.Add(_enemySpawnListSO.tracer, new EnemySpawnData
            {
                _distanceFromPlayer = 150f,
                _spawnChances = 1,
                _score = 75,
                _health = _tracerAttributes._tracerMaxHealth,
                _damageDealAmout = _tracerAttributes._tracerDamageDealAmount,
                _damageGives = _tracerAttributes._tracerDamageGives,
                _moveSpeed = _tracerAttributes._tracer_MoveSpeed,

                _missileInitiation = _tracerAttributes._numOfMissileInitiation,
                _intervalbetweenMissiles = _tracerAttributes._intervalBetweenMissiles,
                _fireRate = _tracerAttributes._fireRate,
                _detectionCheckRadius = _tracerAttributes._playerDetectionCheckRadius,
                _nearCheckRadius = _tracerAttributes._playerNearCheckRadius
            });

            _enemies.Add(_enemySpawnListSO.smasher, new EnemySpawnData
            {
                _distanceFromPlayer = 90f,
                _spawnChances = 2,
                _score = 100,
                _health = _smasherAttributes._smasherMaxHealth,
                _damageDealAmout = _smasherAttributes._smasherDamageDealAmount,
                _damageGives = _smasherAttributes._smasherDamageGives,
                _moveSpeed = _smasherAttributes._smasher_MoveSpeed,

                _playerDetectionDistance = _smasherAttributes._playerDetectionDistance
            });
        }
    }

    private void Start()
    {
        //Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        _player = player?.GetComponent<Transform>();
        if(player == null)
        {
            Debug.Log("Player Not found");
        }

        //Get all the Nodes from the Scene to this list 
        if(AStarManager.instance != null)
        {
            _allNodesInTheScene = AStarManager.instance.AllNodesInTheScene;
        }

        //GetNearestNode(_player.position);
        //Debug.Log(GetNearestNode(_player.position));

        //GetRandomNode(_player.position);


        if (player != null && player.gameObject.activeInHierarchy)
        {
            StartWave();
        }

    }

    public void ObservedUpdate()
    {
        
    }

    #region Enemy Spawn Manager

    public GameObject GetPrefabByEnemyType(EnemyType type)
    {
        if (_enemySpawnListSO == null) return null;

        switch (type)
        {
            case EnemyType.Chaser: return _enemySpawnListSO.chaser;
            case EnemyType.Tracer: return _enemySpawnListSO.tracer;
            case EnemyType.Smasher: return _enemySpawnListSO.smasher;
            default: return null;
        }
    }

    public bool TryGetEnemyData(GameObject prefab, out EnemySpawnData data)
    {
        bool found = _enemies.TryGetValue(prefab, out data);

        if (found)
        {
            Debug.Log(
                $"[EnemyData] Prefab: {prefab.name} | " +
                $"Health: {data._health}, DamageDeal: {data._damageDealAmout}, " +
                $"DamageGives: {data._damageGives}, MoveSpeed: {data._moveSpeed}, " +
                $"DistanceFromPlayer: {data._distanceFromPlayer}, " +
                $"SpawnChances: {data._spawnChances}, Score: {data._score}, " +
                $"MissileInit: {data._missileInitiation}, IntervalBetweenMissiles: {data._intervalbetweenMissiles}, " +
                $"FireRate: {data._fireRate}, DetectRadius: {data._detectionCheckRadius}, " +
                $"NearRadius: {data._nearCheckRadius}, PlayerDetectDist: {data._playerDetectionDistance}"
            );
        }
            return found;
    }

    public void EnemyDestroyed(GameObject enemy)
    {
        _enemiesAlive = Mathf.Max(0, _enemiesAlive - 1);

        if (_enemiesAlive <= 0 && _enemiesSpawnedCount >= _enemiesToSpawn)
        {
            OnWaveComplete();
        }
    }

    private int EnemiesPerWave()
    {
        return Mathf.RoundToInt(_spawnLimitPerWave * Mathf.Pow(_currentWave, _spawnScalingFactor));
    }

    private void StartWave()
    {
        _currentWave++;
        _enemiesToSpawn = EnemiesPerWave();
        _enemiesSpawnedCount = 0;
        _enemiesAlive = 0;

        Debug.Log($"CurrentWave: {_currentWave}, _enemiesToSpawn: {_enemiesToSpawn}, _EnemiesSpawned: {_enemiesSpawnedCount}, _enemiesAlive: {_enemiesAlive}");

        if (_currentWave > 1)
        {
            ApplyWaveScalingToEnemyData();
        }
        StartCoroutine(SpawnWaveCoroutine());
    }

    private void OnWaveComplete()
    {
        _waveActive = false;
        Debug.Log($"Wave {_currentWave} Complete!");

        StartCoroutine(WaveCompleteCoroutine());
    }

    private IEnumerator WaveCompleteCoroutine()
    {
        yield return new WaitForSeconds(_timeBetweenWave);

        //Start next Wave
        StartWave();
    }

    private void ApplyWaveScalingToEnemyData()
    {
        //Compute multiplier based on current wave
        float multiplier = 1f + _currentWave * _spawnScalingFactor;
        float partialMultiplier = 1f + _currentWave * _partialSpawnScalingFactor;

        //Coppy keys so we can safely write back modified structs
        var keys = new List<GameObject>(_enemies.Keys);

        //Loop over all enemy entries
        foreach(var prefab in keys)
        {
            EnemySpawnData data = _enemies[prefab];

            //Spawn Configs
            data._spawnChances = Mathf.RoundToInt(data._spawnChances * multiplier);
            data._score = Mathf.RoundToInt(data._score * multiplier);

            //general & chaser has only these attributes
            data._health = Mathf.RoundToInt(data._health * multiplier);

            data._damageDealAmout = Mathf.RoundToInt(data._damageDealAmout * partialMultiplier);
            data._damageGives = Mathf.RoundToInt(data._damageGives * partialMultiplier);
            data._moveSpeed = Mathf.RoundToInt(data._moveSpeed * partialMultiplier);

            //Tracer
            data._nearCheckRadius = data._nearCheckRadius * partialMultiplier;
            data._detectionCheckRadius = data._detectionCheckRadius * partialMultiplier;
            data._fireRate = data._fireRate * partialMultiplier;
            data._missileInitiation = Mathf.RoundToInt(data._missileInitiation * partialMultiplier);
            data._intervalbetweenMissiles = Mathf.RoundToInt(data._intervalbetweenMissiles * partialMultiplier);

            //Chaser
            data._playerDetectionDistance = Mathf.RoundToInt(data._playerDetectionDistance * partialMultiplier);



            _enemies[prefab] = data;

            Debug.Log(
                "Enemy Data after applying scaling Factor: " +
                $"[EnemyData] Prefab: {prefab.name} | " +
                $"Health: {data._health}, DamageDeal: {data._damageDealAmout}, " +
                $"Spawn Chances: {data._spawnChances}, Score: {data._score}" +
                $"DamageGives: {data._damageGives}, MoveSpeed: {data._moveSpeed}, " +
                $"DistanceFromPlayer: {data._distanceFromPlayer}, " +
                $"SpawnChances: {data._spawnChances}, Score: {data._score}, " +
                $"MissileInit: {data._missileInitiation}, IntervalBetweenMissiles: {data._intervalbetweenMissiles}, " +
                $"FireRate: {data._fireRate}, DetectRadius: {data._detectionCheckRadius}, " +
                $"NearRadius: {data._nearCheckRadius}, PlayerDetectDist: {data._playerDetectionDistance}"
            );
        }
    }

    IEnumerator SpawnWaveCoroutine()
    {
        _waveActive = true;

        while (_enemiesSpawnedCount < _enemiesToSpawn)
        {
            GameObject enemyPrefab = GetRandomEnemyByChances();
            if (enemyPrefab != null)
            {
                Node node = GetRandomNode(_player.position, enemyPrefab);

                if(node != null)
                {
                    GameObject enemy = PoolManager.SpawnObject(enemyPrefab, node.transform.position, Quaternion.identity);

                    _enemiesSpawnedCount++;
                    _enemiesAlive++;
                }
            }
            yield return new WaitForSeconds(_spawnInterval);
        }
    }

    //IEnumerator SpawnRandomEnemyByChances()
    //{
    //    var wait = new WaitForSeconds(_spawnInterval);

    //    while(true)
    //    {
    //        GameObject enemyPrefab = GetRandomEnemyByChances();

    //        if(enemyPrefab != null)
    //        {
    //            Node node = GetRandomNode(_player.position, enemyPrefab);

    //            if (node != null)
    //            {
    //                PoolManager.SpawnObject(enemyPrefab, node.transform.position, Quaternion.identity);
    //            }
    //        }

    //        yield return wait;
    //    }
    //}

    GameObject GetRandomEnemyByChances()
    {
        if(_enemies.Count == 0)
            return null;

        float total = 0f;
        foreach(var value in _enemies)
        {
            total += value.Value._spawnChances;
        }

        if(total <= 0f)
            return null;

        float pick = Random.Range(0f, total);

        foreach(var value in _enemies)
        {
            pick -= value.Value._spawnChances;

            if (pick <= 0f)
                return value.Key;
        }

        foreach(var value in _enemies)
            return value.Key;

        return null;
    }


    Node GetRandomNode(Vector3 Pos, GameObject enemyPrefab)
    {
        if(!_enemies.TryGetValue(enemyPrefab, out EnemySpawnData data))
        {
            Debug.LogWarning($"[GetRandomNode] No EnemySpawnData for {enemyPrefab.name}");
            return null;    
        }

        float distanceFromPlayer = data._distanceFromPlayer;

        List<Node> nodeCandidates = new List<Node>();

        foreach (var node in _allNodesInTheScene)
        {
            float distance = Vector2.Distance(node.transform.position , Pos);

            if (distance <= distanceFromPlayer && node.connectionType == Node.ConnectionType.walkable)
            {
                nodeCandidates.Add(node);
            }
        }

        if (nodeCandidates.Count == 0)
            return null;

        //pick random node from the candidates list
        int index = Random.Range(0, nodeCandidates.Count);
        Debug.Log($"Random Node {nodeCandidates.Count}");
        return nodeCandidates[index];
    }

    #endregion
}
