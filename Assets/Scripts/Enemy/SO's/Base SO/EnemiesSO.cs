using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName ="Enemy/SO's/EnemiesStats")]
public class EnemiesSO : ScriptableObject
{
    public EnemyType enemyTypeforAttributes;

    //Chaser Attributes
    [Header("Chaser Attributes")]
    public int _chaserMaxHealth;
    public int _chaserDamageDealAmount;
    public int _chaser_Movespeed;

    [Header("Tracer Attributes")]
    public int _tracerMaxHealth;
    public int _tracerDamageDealAmount;
    public int _tracer_MoveSpeed;

    public int _numOfMissileInitiation = 2;
    public float _intervalBetweenMissiles = 6f;
    public float _fireRate;

    public float _playerDetectionCheckRadius = 20f;
    public float _playerNearCheckRadius = 30f;

    [Header("Smasher Attributes")]
    public int _smasherMaxHealth;
    public int _smasherDamageDealAmount;
    public int _playerDetectionDistance;
    public float _smasher_MoveSpeed;

}

#if UNITY_EDITOR

[CustomEditor(typeof(EnemiesSO))]
public class EnemiesSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EnemiesSO so = (EnemiesSO)target;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyTypeforAttributes"));

        switch(so.enemyTypeforAttributes)
        {
            case EnemyType.Chaser:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_chaserMaxHealth"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_chaserDamageDealAmount"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_chaser_Movespeed"));
                break;

            case EnemyType.Tracer:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_tracerMaxHealth"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_tracerDamageDealAmount"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_tracer_MoveSpeed"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("_numOfMissileInitiation"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_intervalBetweenMissiles"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_fireRate"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("_playerDetectionCheckRadius"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_playerNearCheckRadius"));
                break;

            case EnemyType.Smasher:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_smasherMaxHealth"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_smasherDamageDealAmount"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_playerDetectionDistance"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_smasher_MoveSpeed"));
                break;
        }

        //Apply Changes to serialized fields
        serializedObject.ApplyModifiedProperties();
    }
}

#endif