using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodFXController : MonoBehaviour
{
    //[Header("Partcile Systems")]
    [SerializeField] private ParticleSystem _impactBurst { get; set; }
    [SerializeField] private ParticleSystem _impactSecondBurst { get; set; }
    [SerializeField] private ParticleSystem _floatingBlood { get; set; }

    //[Header("Droplets")]
    [SerializeField] private GameObject _bloodDropletPrefab { get; set; }

    [Header("Blood SO")]
    [SerializeField] private BloodsSO _bloodSO;

    public static BloodFXController instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

    }

    public void PlayBloodFX(Vector2 hitPoint, Vector2 hitNormal, Vector2 attackDirection, BloodStainSpawnManager.CharacterType characterType)
    {

        //Accessing the enum from stainSpawnManager we are assigning the blood particles according to it
        if (characterType == BloodStainSpawnManager.CharacterType.Enemy)
        {
            _impactBurst = _bloodSO.enemyImpactBurst;
            _impactSecondBurst = _bloodSO.enemySecondImpactBurst;
            _floatingBlood = _bloodSO.enemyFloatingBlood;
            _bloodDropletPrefab = _bloodSO.enemyDropletPrefabs;
        }
        else if (characterType == BloodStainSpawnManager.CharacterType.Player)
        {
            _impactBurst = _bloodSO.playerImpactBurst;
            _impactSecondBurst = _bloodSO.playerSecondImpactBurst;
            _floatingBlood = _bloodSO.playerFloatingBlood;
            _bloodDropletPrefab = _bloodSO.playerDropletPrefabs;
        }

        //Impact Burst
        SpawnImpactBurstParticle(hitPoint, hitNormal);

        //Floating Blood
        SpawnParticles(_floatingBlood, hitPoint, hitNormal);

        //Blood Droplets
        SpawnDroplets(hitPoint, attackDirection, characterType);
    }

    private void SpawnParticles(ParticleSystem ps, Vector2 pos, Vector2 normal)
    {
        PoolManager.SpawnObject(ps, pos, Quaternion.identity, PoolManager.PoolType.ParticleSystem);
        ps.transform.position = pos;
        ps.transform.up = normal;
        ps.Play();
    }

    private void SpawnImpactBurstParticle(Vector2 hitPoint, Vector2 normal)
    {
        var impactBurst = PoolManager.SpawnObject(_impactBurst, hitPoint, Quaternion.identity, PoolManager.PoolType.ParticleSystem);
        var impactBurst2 = PoolManager.SpawnObject(_impactSecondBurst, hitPoint, Quaternion.identity, PoolManager.PoolType.ParticleSystem);

        float zRotation;

        //Check the ceiling and floor of the platform
        if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y))
        {
            //left and rigth side
            zRotation = normal.x > 0 ? -90f : 90f;
        }
        else
        {
            //up and bottom
            zRotation = normal.y > 0 ? 180f : 0f;
        }

        impactBurst.transform.rotation = Quaternion.Euler(0, 0, zRotation);
        impactBurst2.transform.rotation = Quaternion.Euler(0, 0, zRotation);
    }


    private void SpawnDroplets(Vector2 hitPoint, Vector2 attackDirection, BloodStainSpawnManager.CharacterType characterType)
    {
        int dropLetCount = Random.Range(4, 7);

        for (int i = 0; i < dropLetCount; i++)
        {
            Vector2 dir = -attackDirection + Random.insideUnitCircle * 5f;

            Vector2 velocity = dir.normalized * Random.Range(6f, 9f);

            var droplet = PoolManager.SpawnObject(_bloodDropletPrefab, hitPoint, Quaternion.identity, PoolManager.PoolType.BloodDroplet);
            droplet.GetComponent<BloodDroplet>().Init(hitPoint, velocity, characterType);
        }
    }
}
