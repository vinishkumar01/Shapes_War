using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TutorialManager : MonoBehaviour
{
    public bool _enteredEnemyIntroArea;
    [SerializeField] private BoxCollider2D _restrictCollider;
    [SerializeField] private BoxCollider2D _triggerCollider;

    //Player Spawn Point
    public GameObject _playerSpawnPoint;

    //Enemey Spawn Points
    private Transform _spawnPoint;
    [SerializeField] private GameObject _chaserSpawnPoint;
    [SerializeField] private GameObject _smasherSpawnPoint;
    [SerializeField] private GameObject _tracerSpawnPoint;

    // Enemies To Spawn
    [SerializeField] private int _enemiesToSpawn = 2;

    //Dialogue 
    [SerializeField] private DialogueTrigger _dialogueTrigger;
    [SerializeField] private bool _waitingForDialogue = false;
    [SerializeField] private bool _dialogueDone = false;

    //Singleton
    public static TutorialManager instance;

    private void Awake()
    {
        instance = this;

        _enteredEnemyIntroArea = false;
        _restrictCollider.enabled = false;
        GameState.CanPlayerControl = true;
    }

    private void OnEnable()
    {
        DialogueManager.OnTutorialDialogueEnded += OnDialogueEnd;
    }

    private void OnDisable()
    {
        DialogueManager.OnTutorialDialogueEnded -= OnDialogueEnd;
    }

    private void OnDialogueEnd()
    {
        if(_waitingForDialogue)
        {
            _dialogueDone = true;
            _waitingForDialogue = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player has entered the Practice Area");
            _enteredEnemyIntroArea = true;
            _restrictCollider.enabled = true;
            _triggerCollider.enabled = false;

            StartCoroutine(TutorialSequence());
        }
    }

    private IEnumerator TutorialSequence()
    {
        //Chaser--
        yield return StartCoroutine(TriggerDialogueAndWait(EnemyType.Chaser, _dialogueTrigger));
        yield return StartCoroutine(SpawnAndWaitForAllDeaths(EnemyType.Chaser, _enemiesToSpawn));

        //Smasher--
        yield return StartCoroutine(TriggerDialogueAndWait(EnemyType.Smasher, _dialogueTrigger));
        yield return StartCoroutine(SpawnAndWaitForAllDeaths(EnemyType.Smasher, _enemiesToSpawn));

        //Tracer--
        yield return StartCoroutine(TriggerDialogueAndWait(EnemyType.Tracer, _dialogueTrigger));
        yield return StartCoroutine(SpawnAndWaitForAllDeaths(EnemyType.Tracer, _enemiesToSpawn));

        Debug.Log("Tutorial enemy intro complete!");
        _restrictCollider.enabled = false;
    }

    private void SpawnEnemy(EnemyType enemyType)
    {
        GameObject prefab = GameManager._instance.GetPrefabByEnemyType(enemyType);

        if (prefab == null)
        {
            Debug.LogWarning($"No prefab found for {enemyType}");
            return;
        }

        switch(enemyType)
        {
            case EnemyType.Chaser:
                _spawnPoint = _chaserSpawnPoint.transform;
                break;
            case EnemyType.Smasher:
                _spawnPoint = _smasherSpawnPoint.transform;
                break;
            case EnemyType.Tracer:
               _spawnPoint = _tracerSpawnPoint.transform;
                break;
        }

        GameObject enemy = PoolManager.SpawnObject(prefab, _spawnPoint.position, Quaternion.identity, PoolManager.PoolType.GameObjects);

        //Calling the dissolve effect when it spawns
        var enemyScript = enemy.GetComponent<Enemy>();
        enemyScript._dissolveEffect.CallDissolveEffect();

        //Add this prefab to the general list in the gameManager
        GameManager._instance._enemiesGeneralList.Add(enemy);
    }

    private IEnumerator WaitforEnemyDeath()
    {
        //Wait Until the GameManager General list is empty
        yield return new WaitUntil(() => GameManager._instance._enemiesGeneralList.Count == 0);
        yield return null;
    }

    private IEnumerator SpawnAndWaitForAllDeaths(EnemyType enemyType, int count)
    {
        GameState.CanPlayerControl = true;

        for(int i = 0; i < count; i++)
        {
            SpawnEnemy(enemyType);

            //wait until that enemy is gone from the general list
            yield return StartCoroutine(WaitforEnemyDeath());
        }
    }

    private IEnumerator TriggerDialogueAndWait(EnemyType enemyType,DialogueTrigger dialogue)
    {
        _waitingForDialogue = true;
        _dialogueDone = false;

        GameState.CanPlayerControl = false;
        //We trigger the dialouge here!!
        switch(enemyType)
        {
            case EnemyType.Chaser:
                dialogue.TriggerChaserDialouge();
                break;
            case EnemyType.Smasher:
                dialogue.TriggerSmasherDialouge();
                break;
            case EnemyType.Tracer:
                dialogue.TriggerTracerDialouge();
                break;
        }

        yield return new WaitUntil(() => _dialogueDone);
    }
}
