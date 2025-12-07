using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour, IUpdateObserver
{
    public static GameManager _instance;

    [Header("Enemy List")]
    [SerializeField] private EnemySpawnListSO _enemySpawnListSO;

    [Header("Enemy Spawn System")]
    [SerializeField] private Dictionary<GameObject, float> _enemies = new Dictionary<GameObject, float>();
    private Transform _player;
    [SerializeField] private float _distanceFromPlayer;
    private float _spawnInterval = 5f;

    [SerializeField] private List<Node> _allNodesInTheScene = new List<Node>();
    [SerializeField] private List<Node> _allEdgeNodesInTheScene = new List<Node>();

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

        //Add the Enemies to the list
        if(_enemySpawnListSO != null)
        {
            _enemies.Add(_enemySpawnListSO.chaser, _distanceFromPlayer = 30f);
            _enemies.Add(_enemySpawnListSO.smasher, _distanceFromPlayer = 20f);
            _enemies.Add(_enemySpawnListSO.tracer, _distanceFromPlayer = 50f);
        }

        //Get all the Nodes from the Scene to this list 
        if(AStarManager.instance != null)
        {
            _allNodesInTheScene = AStarManager.instance.AllNodesInTheScene;
            Debug.Log($"All Nodes in the scene before adding all the edge nodes to this list {_allNodesInTheScene.Count}");
            _allEdgeNodesInTheScene = AStarManager.instance.AllEdgeNodesInTheScene;
            _allNodesInTheScene.AddRange(_allEdgeNodesInTheScene);
        }

        GetNearestNode(_player.position);
        Debug.Log(GetNearestNode(_player.position));

        //if (player.gameObject.activeInHierarchy)
        //{
        //    StartCoroutine(SpawnEnemy());
        //}

    }

    public void ObservedUpdate()
    {
        
    }

    IEnumerator SpawnEnemy()
    {
        var wait = new WaitForSeconds(_spawnInterval);

        while(true)
        {
            Vector2 SpawnPosition = new Vector2(_distanceFromPlayer, 0);

            var Randomizer = Random.Range(0, _enemies.Count);

            foreach (var enemy in _enemies)
            {
                //PoolManager.SpawnObject(enemy, (Vector2)_player.position + SpawnPosition, Quaternion.identity);
            }

            yield return wait;
        }
        
    }

    Node GetNearestNode(Vector3 Pos)
    {
        Node nearestNode = null;    

        float shortest = float.MaxValue;    

        foreach(var node in _allNodesInTheScene)
        {
            float distance = (Pos - node.transform.position).sqrMagnitude;

            if(distance < shortest)
            {
                shortest = distance;
                nearestNode = node;
            }
        }

        return nearestNode;
    }

    Node GetRandomNode(Vector3 Pos)
    {
        Node farthestNode = null;

        float farthest = float.MaxValue;

        foreach (var node in _allNodesInTheScene)
        {
            var randomizer = Random.Range(-_distanceFromPlayer, _distanceFromPlayer);

            if (randomizer < farthest)
            {
                farthest = randomizer;
                farthestNode = node;
            }
        }
        return farthestNode;
    }

}
