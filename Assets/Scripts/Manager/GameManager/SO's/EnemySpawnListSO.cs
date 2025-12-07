using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameManager/SO's/EnemySpawnerListSO")]
public class EnemySpawnListSO : ScriptableObject
{
    [Header("Enemies")]
    public GameObject chaser;
    public GameObject smasher;
    public GameObject tracer;
}
