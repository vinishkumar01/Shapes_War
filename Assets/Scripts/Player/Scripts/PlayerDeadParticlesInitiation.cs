using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDeadParticlesInitiation : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GameObject[] _bodyParts;
    [SerializeField] private Transform _player;

    [Header("Body parts Attributes")]
    private Rigidbody2D _rb;
    public Coroutine _spawnBodyParticles;

    [Header("Randomness for to apply Force on X and Y direction when the body spawns")]
    [Range(-10, 10)]
    [SerializeField] private float _randomMin = -5f;
    [Range(-10, 10)]
    [SerializeField] private float _randomMax = 7f;

    private Vector2 _force;
    private float _torqueForce;

    public void CallSpawnBodyParticle()
    {
        if(this.isActiveAndEnabled)
        {
            if(_spawnBodyParticles != null)
            {
                StopCoroutine(_spawnBodyParticles);
            }

            _spawnBodyParticles = StartCoroutine(SpawnBodyParticles());
        }
    }

    private IEnumerator SpawnBodyParticles()
    {
        List<GameObject> spawnedBodys = new List<GameObject>();

        foreach (var part in _bodyParts)
        {
            var prefabs = PoolManager.SpawnObject(part, _player.transform.position, Quaternion.identity, PoolManager.PoolType.GameObjects);

            _rb = prefabs.GetComponent<Rigidbody2D>();

            if (_rb == null) continue;

            _rb.isKinematic = false;
            _rb.WakeUp();
            _rb.velocity = Vector2.zero;
            _rb.angularVelocity = 0f;

            _force = new Vector2(Random.Range(_randomMin, _randomMax), Random.Range(_randomMin, _randomMax));
            _torqueForce = Random.Range(_randomMin, _randomMax);
            _rb.AddForce(_force, ForceMode2D.Impulse);
            _rb.AddTorque(_torqueForce, ForceMode2D.Impulse);

            spawnedBodys.Add(prefabs);
        }

        yield return null;
    }

}
