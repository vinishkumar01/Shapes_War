using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager instance;

    [SerializeField] bool _addtoDontDestroyOnLoad = false;

    // Empty GameObject Holders to make the Heirarchy clean
    private GameObject _emptyHolder;
    private static GameObject _particleSystemEmpty;
    private static GameObject _gameObjectsEmpty;
    private static GameObject _soundFXEmpty;
    private static GameObject _nodesEmpty;
    private static GameObject _bloodstainEmpty;
    private static GameObject _bloodDropletEmpty;
    private static GameObject _playerAfterimageEmpty;

    //Dictonary<Key, Value> -- Just incase 
    private static Dictionary<GameObject, ObjectPool<GameObject>> _objectPools;
    private static Dictionary<GameObject, GameObject> _cloneToPrefabMap;

    public enum PoolType
    {
        ParticleSystem,
        GameObjects,
        SoundFX,
        Nodes,
        BloodStains,
        BloodDroplet,
        PlayerAfterimage
    }

    public static PoolType poolingType;


    private void Awake()
    {
        instance = this;

        //Initializing the Dictionary Here
        _objectPools = new Dictionary<GameObject, ObjectPool<GameObject>>();
        _cloneToPrefabMap = new Dictionary<GameObject, GameObject>();   

        SetupEmpties();
    }

    //This function set ups new gameObject in the scene and setting up all the gameObjects as child to the emptygameObject
    void SetupEmpties()
    {
        _emptyHolder = new GameObject("Object Pool");

        _particleSystemEmpty = new GameObject("Particle Effects");
        _particleSystemEmpty.transform.SetParent(_emptyHolder.transform);

        _gameObjectsEmpty = new GameObject("GameObjects");
        _gameObjectsEmpty.transform.SetParent(_emptyHolder.transform);

        _soundFXEmpty = new GameObject("Sound Effects");
        _soundFXEmpty.transform.SetParent(_emptyHolder.transform);

        _nodesEmpty = new GameObject("Nodes");
        _nodesEmpty.transform.SetParent(_emptyHolder.transform);

        _bloodstainEmpty = new GameObject("Blood Stains");
        _bloodstainEmpty.transform.SetParent(_emptyHolder.transform);

        _bloodDropletEmpty = new GameObject("Blood Droplets");
        _bloodDropletEmpty.transform.SetParent(_emptyHolder.transform);

        _playerAfterimageEmpty = new GameObject("Player After Image");
        _playerAfterimageEmpty.transform.SetParent(_emptyHolder.transform);

        if (_addtoDontDestroyOnLoad)
        {
            DontDestroyOnLoad(_particleSystemEmpty.transform.root);
        }
    }


    // This function handle Pooling Behind the Scene 
    static void CreatePool(GameObject prefab, Vector3 pos, Quaternion rot, PoolType poolType = PoolType.GameObjects)
    {
        ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            createFunc: () => CreateObjects(prefab, pos, rot, poolType),
            actionOnGet: OnGetObject,
            actionOnRelease: OnReleaseObject,
            actionOnDestroy: OnDestroyObject
        );

        _objectPools.Add(prefab, pool);

    }

    // This Function handle the Spawning of the Object into the game 
    static GameObject CreateObjects(GameObject prefab, Vector3 pos, Quaternion rot, PoolType poolType = PoolType.GameObjects)
    {
        prefab.SetActive(false);

        GameObject obj = Instantiate(prefab, pos, rot);

        prefab.SetActive(true);

        GameObject parentObject = SetParentObject(poolType);
        obj.transform.SetParent(parentObject.transform);

        return obj;
    }

    static void OnGetObject(GameObject obj)
    {
        //Optional Logic
    }

    //So when On Release method this is putting our objects back to the pool 
    static void OnReleaseObject(GameObject obj)
    {
        obj.SetActive(false);
    }

    static void OnDestroyObject(GameObject obj)
    {
        if(_cloneToPrefabMap.ContainsKey(obj))
        {
            _cloneToPrefabMap.Remove(obj);
        }
    }

    static GameObject SetParentObject(PoolType poolType)
    {
        switch (poolType)
        {
            case PoolType.ParticleSystem:
                return _particleSystemEmpty;

            case PoolType.GameObjects:
                return _gameObjectsEmpty;

            case PoolType.SoundFX:
                return _soundFXEmpty;

            case PoolType.Nodes:
                return _nodesEmpty;

            case PoolType.BloodStains:
                return _bloodstainEmpty;

            case PoolType.BloodDroplet:
                return _bloodDropletEmpty;

            case PoolType.PlayerAfterimage:
                return _playerAfterimageEmpty;

            default:
                return null;
        }
    }

    private static T SpawnObject<T>(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRotation, PoolType poolType = PoolType.GameObjects) where T : Object
    {
        if (!_objectPools.ContainsKey(objectToSpawn))
        {
            CreatePool(objectToSpawn, spawnPos, spawnRotation, poolType);
        }

        GameObject obj = _objectPools[objectToSpawn].Get();

        if(obj != null)
        {
            if(!_cloneToPrefabMap.ContainsKey(obj))
            {
                _cloneToPrefabMap.Add(obj, objectToSpawn);
            }

            obj.transform.position = spawnPos;
            obj.transform.rotation = spawnRotation;
            obj.SetActive(true);

            if(typeof(T) == typeof(GameObject))
            {
                return obj as T;
            }

            T component = obj.GetComponent<T>();

            if(component == null)
            {
                Debug.LogError($"Object {objectToSpawn.name} doesnt have component of type {typeof(T)}");
                return null;
            }

            return component;
        }

        return null;
    }


    //So now that we want to able to handle both componnents and game objects we are going to create 2 overload methods

    // This will actually take a generic type T so we dont have to pass a gameObject in the parameter
    public static T SpawnObject<T>(T typePrefab, Vector3 spawnPos, Quaternion spawnRotation, PoolType poolType = PoolType.GameObjects) where T : Component
    {
        return SpawnObject<T>(typePrefab.gameObject, spawnPos, spawnRotation, poolType);
    }

    // This will handle the gameobject 
    public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRotation, PoolType poolType = PoolType.GameObjects)
    {
        return SpawnObject<GameObject>(objectToSpawn, spawnPos, spawnRotation, poolType);
    }

    public static void ReturnObjectToPool(GameObject obj, PoolType poolType = PoolType.GameObjects)
    {
        if(_cloneToPrefabMap.TryGetValue(obj, out GameObject prefab))
        {
            GameObject parentObject = SetParentObject(poolType);

            if(obj.transform.parent != parentObject.transform)
            {
                // We are reparenting so the object knows where it should go
                obj.transform.SetParent(parentObject.transform);
            }

            if(_objectPools.TryGetValue(prefab , out ObjectPool<GameObject> pool))
            {
                pool.Release(obj);
            }
        }
        else
        {
            Debug.LogWarning("Trying to return an object that is not pooled: " + obj.name);
        }
           
    }
}
