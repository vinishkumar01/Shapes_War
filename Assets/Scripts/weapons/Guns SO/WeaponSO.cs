using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName ="weapons/Guns SO/WeaponsStats")]
public class WeaponSO : ScriptableObject
{
    public WeaponType weaponType;

    [Header("General")]
    [Header("-References-")]
    public GameObject bulletPrefab;
    public LayerMask hitLayer;
    
    [Header("Weapon Attributes")]
    public int magSize;
    public float bulletRange;
    public bool autoReload;
    public float reloadTime;

    public int bulletsLeft;
    public bool isReloading;
    public bool canShoot;

    [Header("Rifle Attributes")]
    public float burstFireRate;
    public int burstCount;
    public float fullAutoFireRate;
    public bool isAutomatic;

    [Header("Grapple Gun")]
    public int grappleLayerNumber;
    public float maxDistance;
    public float launchSpeed;
}

#if UNITY_EDITOR

[CustomEditor(typeof(WeaponSO))]
public class WeaponSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        WeaponSO so = (WeaponSO)target;

        //Displaying other general attributes
        EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletPrefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hitLayer"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("magSize"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletRange"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("autoReload"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadTime"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletsLeft"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isReloading"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("canShoot"));


        //We will select the weapon type here according to that the attributes will be displayed
        EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponType"));

        switch (so.weaponType)
        {
            case WeaponType.Rifle:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("burstFireRate"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("burstCount"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fullAutoFireRate"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isAutomatic"));
                break;

            case WeaponType.GrappleGun:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("grappleLayerNumber"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxDistance"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("launchSpeed"));
                break;
        }

        //Apply Changes to serialized fields
        serializedObject.ApplyModifiedProperties();
    }
}

#endif
