using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSelection : MonoBehaviour, IUpdateObserver
{
    [SerializeField] private int selectedWeapon = 0;
    [SerializeField] private RectTransform RectImage;
    [SerializeField] private RectTransform[] weaponImage;

    private void OnEnable()
    {
        UpdateManager.RegisterObserver(this);
    }

    private void Start()
    {
        SelectWeapon();
    }

    public void ObservedUpdate()
    {
        GoThroughWeapons();
    }

    private void SelectWeapon()
    {
        int i = 0;
        foreach(Transform weapon in transform)
        {
            if(i == selectedWeapon)
            {
                weapon.gameObject.SetActive(true);
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }
            i++;
        }
    }

    private void GoThroughWeapons()
    {
        int previousSelectedWeapon = selectedWeapon;

        if (weaponImage != null && RectImage != null)
        {
            if (Input.GetAxis("Mouse ScrollWheel") < 0f) //Scroll Up
            {
                if (selectedWeapon >= transform.childCount - 1)
                {
                    selectedWeapon = 0;
                }
                else
                {
                    selectedWeapon++;
                }

                UpdateWeaponUI();
            }
            
            if (Input.GetAxis("Mouse ScrollWheel") > 0f || UserInputs.instance._playerInputs.Player.WeaponSwitch.IsPressed()) //Scroll Down
            {
                if (selectedWeapon < 0)
                {
                    selectedWeapon = transform.childCount - 1;
                }
                else
                {
                    selectedWeapon--;
                }

                UpdateWeaponUI();
            }
        }


        if(previousSelectedWeapon != selectedWeapon)
        {
            SelectWeapon();
        }
    }

    private void UpdateWeaponUI()
    {
        selectedWeapon = Mathf.Clamp(selectedWeapon, 0, weaponImage.Length - 1);
        RectImage.position = weaponImage[selectedWeapon].position;
    }

    private void OnDisable()
    {
        UpdateManager.UnregisterObserver(this);
    }
}
