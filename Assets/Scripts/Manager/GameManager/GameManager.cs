using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour, IUpdateObserver
{
    [Header("Reference")]
    public static GameManager _instance;
    private Transform _playerTransform;
    private GameObject _playerPrefab;
    [SerializeField] private List<Node> _allNodesInTheScene = new List<Node>();

    [Header("SO's")]
    [SerializeField] public GameManagerDataSO _gameManagerSpawnListSO;
    //Enemies------
    [SerializeField] private EnemiesSO _chaserAttributes;
    [SerializeField] private EnemiesSO _tracerAttributes;
    [SerializeField] private EnemiesSO _smasherAttributes;
    //Player-------
    [SerializeField] private PlayerDataSO _playerSO;
    [SerializeField] public bool _playerGotInDeathZone = false;
    [SerializeField] public bool _playerGotInSpikesZone = false;

    [Header("Enemy Spawn System")]
    [SerializeField] private Dictionary<GameObject, EnemySpawnData> _enemies = new ();
    [SerializeField] private Dictionary<GameObject, PlayerSpawnData> _playerData = new();
    [SerializeField] private Dictionary<GameObject, PlayerSpawnData> _powerUps = new ();
    
    //Enemy General List
    public List<GameObject> _enemiesGeneralList = new List<GameObject> ();

    [SerializeField] private float _spawnInterval;
    [SerializeField] private float _powerUpSpawnInterval;

    [SerializeField] private List<SmasherSpawnZone> _smasherSpawnZones;
    private List<Node> _cachedSmasherZone = new List<Node> ();

    [Header("Wave System Attributes")]
    public bool _activateWave = false;
    private int _spawnLimitPerWave = 5;
    [SerializeField] private float _timeBetweenWave = 5f;
    [SerializeField] private float _enemySpawnScalingFactor = 0.03f;
    [SerializeField] private float _playerSpawnScalingFactor = 0.05f;

    [SerializeField] private int _currentWave = 0;
    [SerializeField] private int _enemiesToSpawn;
    [SerializeField] private int _enemiesSpawnedCount;
    [SerializeField] private float timeSinceLastSpawn;
    [SerializeField] private int _enemiesAlive = 0;
    [SerializeField] public bool _waveActive = false;
    [SerializeField] private bool _spawningCompleted = false;
    [SerializeField] private bool _waveCompletedTriggered = false;
    [SerializeField] private bool _waitingForNextWave = false;
    [SerializeField] private bool _healthUpdatedInWave;
    private Coroutine _spawnCoroutine;

    [Header("Score")]
    private int _score;
    private int _highScore;

    [Header("Missile Lists for TargetIndicators")]
    public List<GameObject> _missilesList = new List<GameObject>();

    [Header("Dialogue Configs")]
    [SerializeField] private DialogueTrigger _dialogueTrigger;
    private bool _dialogueTriggered = false;
    private Coroutine _dialogueCoroutine;

    [Header("Scene")]
    private string currentSceneName;

    [Header("SFX for Re-spawn")]
    [SerializeField] private AudioClip _respawnSFX;

    #region Custom Enemy Data
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
        public int _missileAttackInitiation;
        public float _intervalbetweenMissiles;
        public float _fireRate;

        public float _detectionCheckRadius;
        public float _nearCheckRadius;

        //Smasher
        public int _playerDetectionDistance;
        public int _smasherWaveAttack;
    }
    #endregion

    #region Custom Player Data
    public struct PlayerSpawnData
    {
        public int playerMaxHealth;
        public bool doubleJumpSkill;
        public bool dashSkill;

        public int doubleJumpCount;
        public int dashCount;
        public int grappleAmmo;
        public int healthPack;
    }

    #endregion

    private void OnEnable()
    {

        SceneManager.sceneLoaded += OnSceneLoaded;
        UpdateManager.RegisterObserver(this);

        DialogueManager.OnDialogueEnded += StartWaveFromDialouge;

        Debug.Log("[GameManager] OnEnable completed, event subscribed");

        if (_currentWave <= 0)
        {
            //Reset the count and the UI on Every Enable
            _playerSO.doubleJumpCount = 10;
            _playerSO.dashCount = 10;
            _playerSO._grappleAmmo = 10;
            _playerSO.lives = _playerSO.maxLives;

            _playerSO.dashSkill = true;
            _playerSO.doubleJumpSkill = true;

            //Reset the Score on beginning 
            _score = 0;
            _highScore = _gameManagerSpawnListSO.highScore;
        }
    }

    private void OnDisable()
    {
        //just in case we are resetting every thing to make sure the next session works fine:
        GameState.CanPlayerControl = false;
        _activateWave = false;
        _waveActive = false;
        _spawningCompleted = false;
        _waveCompletedTriggered = false;
        _enemiesSpawnedCount = 0;
        _enemiesAlive = 0;
        _currentWave = 0;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        UpdateManager.UnregisterObserver(this);

        DialogueManager.OnDialogueEnded -= StartWaveFromDialouge;
    }

    #region Scene Configs

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;

        // If the survival scene is detected we are following the wave system
        if(scene_Manager._instance.IsGameplayScenes(currentSceneName))
        {
            GameState.CanPlayerControl = false;
            _activateWave = false;
            _waveActive = false;
            _spawningCompleted = false;
            _waveCompletedTriggered = false;
            _enemiesSpawnedCount = 0;
            _enemiesAlive = 0;
            _currentWave = 0;

            //Stopping any lingering Spawn coroutine from any previous scenes!
            if(_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }

            //clearing the general list in case if there are any object from the previous scene
            _enemiesGeneralList.Clear();

            _dialogueTriggered = false; // reset per scene

            if(_dialogueCoroutine != null)
            {
                StopCoroutine(_dialogueCoroutine);
            }

            _dialogueCoroutine = StartCoroutine(InitSceneAndTriggerDialogue());

        }
    }

    private IEnumerator InitSceneAndTriggerDialogue()
    {
        //wait for a frame so that the start method in the scne gets initiated
        yield return null;

        //Re-find the player and Nodes fresh for the scene
        _playerPrefab = GameObject.FindGameObjectWithTag("Player");
        _playerTransform = _playerPrefab?.transform;

        //Re-assign node from the scene so that we get the nodes fresh
        if (AStarManager.instance != null)
        {
            _allNodesInTheScene = AStarManager.instance.AllNodesInTheScene;
        }


        //Re - cache the spawn zone for the smasher enemy
        cachedZone();

        yield return new WaitForSeconds(0.5f);

        if(_dialogueTriggered) yield break ;

        _dialogueTriggered = true;
        _dialogueTrigger.TriggerDialouge();

    }

    private void StartWaveFromDialouge()
    {
        Debug.Log($"[GameManager] StartWaveFromDialogue called! " +
              $"playerNull:{_playerPrefab == null} " +           // ADD
              $"playerActive:{_playerPrefab?.activeInHierarchy}");

        GameState.CanPlayerControl = true;
        _activateWave = true;
        _waveActive= false;

        Debug.Log($"CanPlayerControl {GameState.CanPlayerControl}: " + $"activate Wave {_activateWave}: " + $"waveActive {_waveActive}: ");
    }

    #endregion

    private void Awake()
    {
        if(_instance !=  null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        #region Data Initialization

        //Add the Enemies to the list
        if (_gameManagerSpawnListSO != null)
        {
            //-----------Enemy----------
            //Get the data from the SO's 
            _enemies.Add(_gameManagerSpawnListSO.chaser, new EnemySpawnData
            {
                _distanceFromPlayer = 50f,
                _spawnChances = 0,
                _score = 50,
                _health = _chaserAttributes._chaserMaxHealth,
                _damageDealAmout = _chaserAttributes._chaserDamageDealAmount,
                _damageGives = _chaserAttributes._chaserDamageGives,
                _moveSpeed = _chaserAttributes._chaser_Movespeed
            });

            _enemies.Add(_gameManagerSpawnListSO.tracer, new EnemySpawnData
            {
                _distanceFromPlayer = 150f,
                _spawnChances = 5,
                _score = 75,
                _health = _tracerAttributes._tracerMaxHealth,
                _damageDealAmout = _tracerAttributes._tracerDamageDealAmount,
                _damageGives = _tracerAttributes._tracerDamageGives,
                _moveSpeed = _tracerAttributes._tracer_MoveSpeed,

                _missileInitiation = _tracerAttributes._numOfMissileInitiation,
                _missileAttackInitiation = _tracerAttributes._numOfMissileAttackInitiation,
                _intervalbetweenMissiles = _tracerAttributes._intervalBetweenMissiles,
                _fireRate = _tracerAttributes._fireRate,
                _detectionCheckRadius = _tracerAttributes._playerDetectionCheckRadius,
                _nearCheckRadius = _tracerAttributes._playerNearCheckRadius
            });

            _enemies.Add(_gameManagerSpawnListSO.smasher, new EnemySpawnData
            {
                _distanceFromPlayer = 90f,
                _spawnChances = 0,
                _score = 100,
                _health = _smasherAttributes._smasherMaxHealth,
                _damageDealAmout = _smasherAttributes._smasherDamageDealAmount,
                _damageGives = _smasherAttributes._smasherDamageGives,
                _smasherWaveAttack = _smasherAttributes._smasherWaveAttackDamageGives,
                _moveSpeed = _smasherAttributes._smasher_MoveSpeed,

                _playerDetectionDistance = _smasherAttributes._playerDetectionDistance
            });

            //------------------Player--------------
            _playerData.Add(_gameManagerSpawnListSO.player, new PlayerSpawnData
            {
                playerMaxHealth = _playerSO.maxHealth,

            });

            //------------PowerUps-----------------
            _powerUps.Add(_gameManagerSpawnListSO.doubleJump, new PlayerSpawnData
            {
                doubleJumpSkill = _playerSO.doubleJumpSkill,
                doubleJumpCount = _playerSO.doubleJumpCount
            });

            _powerUps.Add(_gameManagerSpawnListSO.dash, new PlayerSpawnData
            {
                dashSkill = _playerSO.dashSkill,
                dashCount = _playerSO.dashCount
            });

            _powerUps.Add(_gameManagerSpawnListSO.grappleAmmo, new PlayerSpawnData
            {
                grappleAmmo = _playerSO._grappleAmmo
            });

            _powerUps.Add(_gameManagerSpawnListSO.healthPack, new PlayerSpawnData
            {
                healthPack = _playerSO.healthPack
            });
        }

        #endregion
    }

    private void Start()
    {
        //Find the player
         _playerPrefab = GameObject.FindGameObjectWithTag("Player");
        _playerTransform = _playerPrefab?.GetComponent<Transform>();
        if(_playerPrefab == null)
        {
            Debug.Log("Player Not found");
            GameObject.FindGameObjectWithTag("Player");
            _playerTransform = _playerPrefab?.transform;
        }

        //Get all the Nodes from the Scene to this list 
        if(AStarManager.instance != null)
        {
            _allNodesInTheScene = AStarManager.instance.AllNodesInTheScene;
        }

        //We are caching the selected zones here for the smasher
        cachedZone();

        //Sets the initial Score at the beginning  
        UpdateScoreUI();

        //updating the UI
        UIManager.InvokeDoubleJumpUpdate(_playerSO.doubleJumpCount);
        UIManager.InvokeDashCoolDownUpdate(0f, _playerSO.dashCoolDown, _playerSO.dashCount);
        UIManager.InvokeGrappleUpdate(_playerSO._grappleAmmo);
        UIManager.InvokeLivesUpdate(_playerSO.lives);
        UIManager.InvokeScoreUpdate(_highScore, _score);
        UIManager.InvokeWaveAndEnemyCountUpdate(_currentWave, _enemiesAlive);

    }

    public void ObservedUpdate()
    {
        if (!_waveActive && !_waitingForNextWave &&_activateWave && _playerPrefab != null && _playerPrefab.gameObject.activeInHierarchy)
        {
            if (_activateWave)
            {                
                StartWave();
            }
        }
        else if (_activateWave && !_waveActive)
        {
            Debug.Log($"[GameManager] Wave blocked — waveActive:{_waveActive} " +
                      $"activateWave:{_activateWave} " +  // ADD THIS
                      $"playerNull:{_playerPrefab == null} " +
                      $"playerInactive:{_playerPrefab != null && !_playerPrefab.gameObject.activeInHierarchy}");
        }
    }
	
#region Spawn Manager-------------------
	 
	IEnumerator SpawnWaveCoroutine()
    {
        _spawningCompleted = false;

        while (_enemiesSpawnedCount < _enemiesToSpawn)
        {
            GameObject enemyPrefab = GetRandomEnemyByChances();
            if (enemyPrefab != null)
            {
                Node node = GetRandomNode(_playerTransform.position, enemyPrefab);

                if(node == null)
                {
                    yield return null;
                    continue;
                }

                if (node != null)
                {
                    GameObject enemy = PoolManager.SpawnObject(enemyPrefab, node.transform.position, Quaternion.identity);

                    //This isn't legit though but this is a temproary fix (I just ruined the enemies encapsulation i guess)
                    var enemyScript = enemy.GetComponent<Enemy>();

                    //Calling the Dissolve effect when instantiated
                    enemyScript._dissolveEffect.CallDissolveEffect();

                    //Adding the enemy to general list just to cross verify
                    _enemiesGeneralList.Add(enemy);
                    Debug.Log($"Enemies Added to the General List {enemy}");
                    
                    _enemiesSpawnedCount++;
                    _enemiesAlive++;

                    UIManager.InvokeWaveAndEnemyCountUpdate(_currentWave, _enemiesAlive);

                    if (_enemiesSpawnedCount >= _enemiesToSpawn)
                    {
                        _spawningCompleted = true;
                    }
                }
            }
            yield return new WaitForSeconds(_spawnInterval);
        }
    }
 
    private void StartWave()
    {
        if(_waveActive)
        {
            return;
        }

        _waveActive = true;

        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }

        _enemiesGeneralList.Clear();
        _spawningCompleted = false;
        _waveCompletedTriggered = false;
        _enemiesSpawnedCount = 0;
        _enemiesAlive = 0;

        if (_currentWave <= 1)
        {
            // Setting the max lives for the player when starting the game.
            _playerSO.lives = _playerSO.maxLives;
            Debug.Log($"Player Lives: {_playerSO.lives}");
        }
        _currentWave++;
        _enemiesSpawnedCount = 0;
        _enemiesAlive = 0;

        UIManager.InvokeWaveAndEnemyCountUpdate(_currentWave, _enemiesAlive);

        ConfigureWave(_currentWave);

        //Debug.Log($"CurrentWave: {_currentWave}, _enemiesToSpawn: {_enemiesToSpawn}, _EnemiesSpawned: {_enemiesSpawnedCount}, _enemiesAlive: {_enemiesAlive}");

        //Start Applying scaling Factor from Wave 6
        if (_currentWave > 5)
        {
            ApplyWaveScalingToEnemyData();
            ApplyWaveScalingToPlayerData();
        }

        _waveActive = true;

        _spawnCoroutine = StartCoroutine(SpawnWaveCoroutine());

        StartCoroutine(PowerUpsSpawnCoroutine());
    }

    private void OnWaveComplete()
    {
        _waveActive = false;
        _waitingForNextWave = true;
        Debug.Log($"Wave {_currentWave} Complete!");

        StartCoroutine(WaveCompleteCoroutine());
    }

    private IEnumerator WaveCompleteCoroutine()
    {
        yield return new WaitForSeconds(_timeBetweenWave);

        _waitingForNextWave = false;

        //Start next Wave
        StartWave();
    }
    #endregion

    #region PowerUp Manager

    IEnumerator PowerUpsSpawnCoroutine()
    {
        while(_waveActive)
        {
            //Debug.Log("[PowerUp] Loop tick, _waveActive = " + _waveActive);

            GameObject powerUpPrefab = GetRandomPowerUps();

            if(powerUpPrefab != null)
            {
                Node node = GetRandomNodeForPowerUps(_playerTransform.position, powerUpPrefab);

                //Debug.Log("[PowerUp] Spawning " + powerUpPrefab.name + " at " + node.transform.position);

                if (node != null)
                {
                    PoolManager.SpawnObject(powerUpPrefab, node.transform.position, Quaternion.identity);
                }
            }

            yield return new WaitForSeconds(_powerUpSpawnInterval);
        }

        Debug.Log("[PowerUp] Coroutine exited, _waveActive = " + _waveActive);
    }

    Node GetRandomNodeForPowerUps(Vector3 Pos, GameObject powerUpPrefab)
    {
        if(!_powerUps.TryGetValue(powerUpPrefab, out PlayerSpawnData data))
        {
            Debug.Log($"[GetRandomNode] No PlayerSpawnData for {powerUpPrefab.name}");
            return null;
        }

        float distanceFromPlayer = Random.Range(20, 140);

        List<Node> nodeCandidates = new List<Node>();

        foreach(var node in _allNodesInTheScene)
        {
            float distance = Vector2.Distance(node.transform.position, Pos);

            if(distance <= distanceFromPlayer && node.connectionType == Node.ConnectionType.walkable)
            {
                nodeCandidates.Add(node);
            }
        }

        if (nodeCandidates.Count == 0)
            return null;

        //Pick Random Node from the node list
        int index = Random.Range(0, nodeCandidates.Count);
        return nodeCandidates[index];
    }

    private GameObject GetRandomPowerUps()
    {
        if (_powerUps == null || _powerUps.Count == 0)
            return null;

        var keys = new List<GameObject>(_powerUps.Keys);

        int index = Random.Range(0, keys.Count);

        return keys[index];
    }

    #endregion

    #region Enemy spawn Configs

    private void cachedZone()
    {
        foreach(var node in _allNodesInTheScene)
        {
            foreach(var zone in _smasherSpawnZones)
            {
                if(zone.containsNode(node.transform.position))
                {
                    _cachedSmasherZone.Add(node);
                    break;
                }
            }
        }

        Debug.Log($"Cached Smasher Zone Node : {_cachedSmasherZone.Count}");
    }

    //Get Nodes from specified area for Smasher to Spawn
    Node GetNodesInsideTheZone(Vector3 playerPosition, float distanceFromPlayer)
    {
        List<Node> validNodes = new List<Node>();

        foreach (var node in _cachedSmasherZone)
        {
            if (node.connectionType == Node.ConnectionType.walkable)
            {
                validNodes.Add(node);
            }
        }

        if (validNodes.Count == 0)
            return null;

        int index = Random.Range(0, validNodes.Count);

        return validNodes[index];
    }

    Node GetRandomNode(Vector3 Pos, GameObject enemyPrefab)
    {
        if (!_enemies.TryGetValue(enemyPrefab, out EnemySpawnData data))
        {
            Debug.LogWarning($"[GetRandomNode] No EnemySpawnData for {enemyPrefab.name}");
            return null;
        }

        float distanceFromPlayer = data._distanceFromPlayer;

        List<Node> nodeCandidates = new List<Node>();

        if (enemyPrefab == _gameManagerSpawnListSO.smasher)
        {
            //Debug.Log($"Zones Count: {_smasherSpawnZones.Count}");

            Node zoneNodes = GetNodesInsideTheZone(Pos, distanceFromPlayer);

            if(zoneNodes == null)
            {
                return null;
            }

            if(zoneNodes != null)
            {
                nodeCandidates.Add(zoneNodes);
            }
        }
        else
        {
            foreach (var node in _allNodesInTheScene)
            {
                float distance = Vector2.Distance(node.transform.position, Pos);

                if (distance <= distanceFromPlayer && node.connectionType == Node.ConnectionType.walkable)
                {
                    nodeCandidates.Add(node);
                }
            }
        }

        if (nodeCandidates.Count == 0)
            return null;

        //pick random node from the candidates list
        int index = Random.Range(0, nodeCandidates.Count);
        //Debug.Log($"Random Node {nodeCandidates.Count}");
        return nodeCandidates[index];
    }

    //Get the Enemy prefab and other details
    public GameObject GetPrefabByEnemyType(EnemyType type)
    {
        if (_gameManagerSpawnListSO == null) return null;

        switch (type)
        {
            case EnemyType.Chaser: return _gameManagerSpawnListSO.chaser;
            case EnemyType.Tracer: return _gameManagerSpawnListSO.tracer;
            case EnemyType.Smasher: return _gameManagerSpawnListSO.smasher;
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


    private int EnemiesPerWave()
    {
        if(_currentWave > 10)
            return Mathf.RoundToInt(_spawnLimitPerWave * Mathf.Pow(_currentWave, _enemySpawnScalingFactor));

        return _spawnLimitPerWave;
    }

    public void EnemyDestroyed(GameObject enemy)
    {
        //Removing that specific Enemy from the list
        _enemiesGeneralList.Remove(enemy);
        _enemiesAlive--;
        UIManager.InvokeWaveAndEnemyCountUpdate(_currentWave, _enemiesAlive);
        if (_waveActive && _spawningCompleted && _enemiesSpawnedCount >= _enemiesToSpawn && _enemiesGeneralList.Count == 0 && !_waveCompletedTriggered)
        {
            _waveCompletedTriggered = true; 
            OnWaveComplete();
        }

        if(_enemiesGeneralList.Count == 0)
        {
            _enemiesAlive = 0;
        }

        Debug.Log($"[WaveCheck] Spawned: {_enemiesSpawnedCount}/{_enemiesToSpawn}, " +
          $"Alive: {_enemiesGeneralList.Count}, " +
          $"SpawningDone: {_spawningCompleted}");
    }

#region Enemy Spawn Chances manual Configs:

    private void SetSpawnChances(int chaserChance, int smasherChance, int tracerChance)
    {
        if (_gameManagerSpawnListSO == null) return;

        if(_enemies.TryGetValue(_gameManagerSpawnListSO.chaser, out var chaser))
        {
            chaser._spawnChances = chaserChance;
            _enemies[_gameManagerSpawnListSO.chaser] = chaser;
        }

        if (_enemies.TryGetValue(_gameManagerSpawnListSO.smasher, out var smasher))
        {
            smasher._spawnChances = smasherChance;
            _enemies[_gameManagerSpawnListSO.smasher] = smasher;
        }

        if (_enemies.TryGetValue(_gameManagerSpawnListSO.tracer, out var tracer))
        {
            tracer._spawnChances = tracerChance;
            _enemies[_gameManagerSpawnListSO.tracer] = tracer;
        }
    }

    private void ConfigureWave(int wave)
    {
        _enemiesToSpawn = EnemiesPerWave();

        //Lets Reset Spawn Chances first
        SetSpawnChances(0, 0, 0);

        if(wave >= 1 && wave <= 5)
        {
            _enemiesToSpawn = 5;

            switch (wave)
            {
                case 1:
                    SetSpawnChances(10, 0, 100); // Chances for Chaser 100%, Smasher 0%, Tracer 0%
                    break;
                case 2:
                case 3:
                case 4:
                    SetSpawnChances(80, 10, 0);
                    break;
                case 5:
                    SetSpawnChances(70, 10, 10);
                    break;
            }
        }

        else if(wave >= 6 && wave <= 10)
        {
            _enemiesToSpawn = 8;

            switch(wave)
            {
                case 6:
                    SetSpawnChances(80, 10, 10);    //Chance for Chaser 80%, Smasher 10%, Tracer 10%
                    break;
                case 7:
                    SetSpawnChances(70, 20, 10); 
                    break;
                case 8:
                case 9:
                case 10:
                    SetSpawnChances(60, 30, 10);
                    break;
            }
        }

        else if(wave >= 11)
        {
            SetSpawnChances(60, 30, 10);
            _enemiesToSpawn = 12;
        }
    }

#endregion

    private void ApplyWaveScalingToEnemyData()
{
        //Compute multiplier based on current wave
        float multiplier = 1f + _currentWave * _enemySpawnScalingFactor;

        Debug.Log($"multiplier: {multiplier}");

        //Coppy keys so we can safely write back modified structs
        var keys = new List<GameObject>(_enemies.Keys);

        //Loop over all enemy entries
        foreach(var prefab in keys)
        {
            EnemySpawnData data = _enemies[prefab];

            //Spawn Configs
            if(_currentWave >= 11)
            {
                data._spawnChances = Mathf.RoundToInt(data._spawnChances * multiplier);
            }
            
            data._score = Mathf.RoundToInt(data._score * multiplier);

            //general & chaser has these attributes
            int healthBaseValue = data._health;
            int healthScaled = Mathf.RoundToInt(data._health * multiplier);
            data._health = Mathf.Clamp(healthScaled, healthBaseValue, 1000);

            int DDBaseValue = data._damageDealAmout;
            int DDScaled = Mathf.RoundToInt(data._damageDealAmout * multiplier);
            data._damageDealAmout = Mathf.Clamp(DDScaled, DDBaseValue, 150);

            int DGBaseValue = data._damageGives;
            int WaveDGBaseValue = data._smasherWaveAttack; // Smasher Wave Attack : Base Value
            int DGScaled = Mathf.RoundToInt(data._damageGives * multiplier);
            int WaveDGScaled = Mathf.RoundToInt(data._smasherWaveAttack * multiplier); // smasher wave attack, scaling value
            data._damageGives = Mathf.Clamp(DGScaled, DGBaseValue, 100);
            data._smasherWaveAttack = Mathf.Clamp(WaveDGScaled, WaveDGBaseValue, 100);

            data._moveSpeed = Mathf.RoundToInt(data._moveSpeed * multiplier);

            //Clamp ma MoveSpeed for each enemy
            if(prefab == _gameManagerSpawnListSO.chaser)
            {
                data._moveSpeed = Mathf.Clamp(data._moveSpeed, 5, 15);
            }
            if (prefab == _gameManagerSpawnListSO.smasher)
            {
                data._moveSpeed = Mathf.Clamp(data._moveSpeed, 500, 800);
            }
            if (prefab == _gameManagerSpawnListSO.tracer)
            {
                data._moveSpeed = Mathf.Clamp(data._moveSpeed, 7, 14);
            }

            //Tracer
            data._nearCheckRadius = data._nearCheckRadius * multiplier;
            data._detectionCheckRadius = data._detectionCheckRadius * multiplier;

            //Clamping nearCheckRadius and detectionCheckRadius
            data._nearCheckRadius = Mathf.Clamp(data._nearCheckRadius, 10, 40);
            data._detectionCheckRadius = Mathf.Clamp(data._detectionCheckRadius, 5, 20);

            //Clamp the missile Initiation so that it never exceeds more than 4
            int baseValue = data._missileInitiation;
            int scaled = Mathf.RoundToInt(data._missileInitiation * multiplier);
            data._missileInitiation = Mathf.Clamp(scaled, baseValue , 4);

            float intervalValue = data._intervalbetweenMissiles;
            float intervalScaled = intervalValue / multiplier;
            data._intervalbetweenMissiles = Mathf.Max(intervalScaled, 1);
    
            //Chaser
            data._playerDetectionDistance = Mathf.RoundToInt(data._playerDetectionDistance * multiplier);

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

    private GameObject GetRandomEnemyByChances()
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

#endregion

//Player Spawn Manager
#region Get player Data
    public GameObject GetPlayerPrefab()
    {
        return _gameManagerSpawnListSO.player;
    }

    public bool TryGetPlayerData(GameObject prefab, out PlayerSpawnData data)
    {
        bool found = _playerData.TryGetValue(prefab, out data);

        if (found)
        {
            Debug.Log(
            $"[EnemyData] Prefab: {prefab.name} | " +
            $"Health: {data.playerMaxHealth}, dash Skill: {data.dashSkill}, " +
            $"double Jump skill: {data.doubleJumpSkill}, dash Count: {data.dashCount}, " +
            $"double Jump Count: {data.doubleJumpCount}, " +
            $"grapple Ammo: {data.grappleAmmo}, health Pack: {data.healthPack}"
            );
        }
        return found;
    }

#endregion

#region Player Health, Respawn and Scaling attributes

    private void ApplyWaveScalingToPlayerData()
    {
        //Compute multiplier based on current wave
        float multiplier = 1f + _currentWave * _playerSpawnScalingFactor;

        Debug.Log($"multiplier: {multiplier}");

        //Coppy keys so we can safely write back modified structs
        var keys = new List<GameObject>(_playerData.Keys);

        //Loop over all enemy entries
        foreach (var prefab in keys)
        {
        
            PlayerSpawnData data = _playerData[prefab];

            int baseHealth = data.playerMaxHealth;
            int healthScaled = Mathf.RoundToInt(multiplier * data.playerMaxHealth);
            data.playerMaxHealth = Mathf.Clamp(healthScaled, baseHealth, 1000);
            _healthUpdatedInWave = true;

            int baseDJCount = data.doubleJumpCount;
            int DJCountScaled = Mathf.RoundToInt(multiplier * data.doubleJumpCount);
            data.doubleJumpCount = Mathf.Clamp(DJCountScaled, baseDJCount, 20);

            int baseDashCount = data.dashCount;
            int DashCountScaled = Mathf.RoundToInt(multiplier * data.dashCount);
            data.dashCount = Mathf.Clamp(DashCountScaled, baseDashCount, 20);

            int baseGrapple = data.grappleAmmo;
            int GrappleScaled = Mathf.RoundToInt(multiplier * data.grappleAmmo);
            data.grappleAmmo = Mathf.Clamp(GrappleScaled, baseGrapple, 30);

            Debug.Log($"health updated in wavw: {_healthUpdatedInWave}");
            Debug.Log(
           $"[plyer data] Prefab: {prefab.name} | " +
           $"Health: {data.playerMaxHealth}, dash Skill: {data.dashSkill}, " +
           $"double Jump skill: {data.doubleJumpSkill}, dash Count: {data.dashCount}, " +
           $"double Jump Count: {data.doubleJumpCount}, " +
           $"grapple Ammo: {data.grappleAmmo}, health Pack: {data.healthPack}"
           );

            _playerData[prefab] = data;
        }
    }

    public void CheckPlayerHealthEveryFrame(Player player)
    {
        if(!_healthUpdatedInWave)
            return;

        //Get the data from the dictionary only the maxHealth
        if(!_playerData.TryGetValue(_gameManagerSpawnListSO.player, out var data))
            return;

        int previousMaxHealth = player.MaxHealth;   
        int updatedMaxHealth = data.playerMaxHealth;

        if (player.CurrentHealth >= previousMaxHealth)
        {
            player.MaxHealth = updatedMaxHealth;
            player.CurrentHealth = updatedMaxHealth;
            Debug.Log($"Health Updated To Max as the current health was hundered: {player.CurrentHealth},  max Health: {player.MaxHealth}");
        }
        else if (player.CurrentHealth < previousMaxHealth)
        {
            player.MaxHealth = updatedMaxHealth;
            Debug.Log($"Health Updated as the currentHealth was less so the health is upgraded to max health, current health: {player.CurrentHealth}, max Health: {player.MaxHealth}");
        }

        player.UpdateHealth();

        _healthUpdatedInWave = false;
    }

    public void AddHealthToPlayer(Player player)
    {
        if (!_powerUps.TryGetValue(_gameManagerSpawnListSO.healthPack, out var data))
            return;

            int baseValue = data.healthPack;
            int value = player.CurrentHealth += data.healthPack;
            player.CurrentHealth = Mathf.Clamp(value, baseValue, player.MaxHealth);
    }

    public void OnPlayerDiedButHasLives(Player player)
    {
        StartCoroutine(RespawnPlayerCoroutine(player));
    }

    private Node GetNearestNodeFromDeathZone(Player player)
    {
        int range = 5;

        Vector3 deathPos = player.lastDeathPosition;

        List<Node> sortedNodes = new List<Node>(_allNodesInTheScene);

        sortedNodes.Sort((a, b) =>
        {
            float da = (deathPos - a.transform.position).sqrMagnitude;
            float db = (deathPos - b.transform.position).sqrMagnitude;
            return da.CompareTo(db);
        });

        int count = Mathf.Min(range, sortedNodes.Count);

        int index = Random.Range(0, count);

        return sortedNodes[index];
    }

    private IEnumerator RespawnPlayerCoroutine(Player player)
    {
        yield return new WaitForSeconds(1.5f);

        if (scene_Manager._instance.IsTutorialScene(currentSceneName) && !TutorialManager.instance._enteredEnemyIntroArea && _playerGotInDeathZone)
        {
            player.transform.position = TutorialManager.instance._playerSpawnPoint.transform.position;
        }
        else if (_playerGotInDeathZone || _playerGotInSpikesZone)
        {
            Node node = GetNearestNodeFromDeathZone(player);

            player.transform.position = node.transform.position;
        }
        else
        {
            player.transform.position = player.lastDeathPosition;
        }

        //SFX for player Re-birth
        SFXManager._instance.playSFX(_respawnSFX, player.transform.position, 1f, false, false);

        player.gameObject.SetActive(true);

        // Lets call the Shock Wave here
        var shockWave = PoolManager.SpawnObject(player._shockWaveScreen, player.transform.position, Quaternion.identity, PoolManager.PoolType.ParticleSystem);
        //player._shockWaveScreen.SetActive(true);
        var shockWaveScript = shockWave.GetComponent<ShockWaveBehaviour>();
        shockWaveScript.CallShockWave();

        //Dissolve Effect will be called here
        player._flashEffect.CallDissolveEffect();

        player.ResetPlayerHealth();
    }

#endregion

    public void ONPlayerGameOver(Player player)
    {
        Debug.Log("Game Over");
    }

    #region Score

    private void UpdateScoreUI()
    {
        UIManager.InvokeScoreUpdate(_highScore, _score);
    }

    public void AddScoreForEnemy(EnemyType enemy)
    {
        GameObject prefab = GetPrefabByEnemyType(enemy);

        if(prefab == null) return;

        if(!_enemies.TryGetValue(prefab, out var data))
            return;

        _score += data._score;
      
        if(_score > _highScore)
        {
            _gameManagerSpawnListSO.highScore = _score;
        }

        UpdateScoreUI();
    }

    #endregion

    #region Condition For Indicators

    public bool IfRemainingEnemies()
    {
        if(_spawningCompleted && _enemiesGeneralList != null && _enemiesGeneralList.Count <= 2)
            return true;

        return false;
    }

    #endregion

    #region Tutorial

    #endregion
}
