using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scripts/blood FX/bloods SO/bloods")]
public class BloodsSO : ScriptableObject
{
    [Header("Enemy")]
    public ParticleSystem enemyImpactBurst;
    public ParticleSystem enemySecondImpactBurst;
    public ParticleSystem enemyFloatingBlood;

    public GameObject enemyDropletPrefabs;
    public GameObject enemyStainPrefab;


    [Header("Player")]
    public ParticleSystem playerImpactBurst;
    public ParticleSystem playerSecondImpactBurst;
    public ParticleSystem playerFloatingBlood;

    public GameObject playerDropletPrefabs;
    public GameObject playerStainPrefab;
}
