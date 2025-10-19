using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponBehaviour : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] Transform GunBarrel;
    [SerializeField] GameObject bulletTrail;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] LayerMask hitLayer;
    [SerializeField] TextMeshProUGUI ModeText;
    [SerializeField] TextMeshProUGUI BulletCountText;
    [SerializeField] TextMeshPro ReloadText;

    [Header("Weapon Stats")]
    [SerializeField] int MagSize;
    [SerializeField] float BurstFireRate;
    [SerializeField] float FullAutoFireRate;
    [SerializeField] protected bool isAutomatic;
    [SerializeField] float BulletRange = 50f;
    [SerializeField] int BurstCount = 3;

    [Header("Reloading")]
    [SerializeField] bool AutoReload;
    [SerializeField] float ReloadTime;

    int BulletsLeft;
    bool isReloading;
    bool CanShoot;

    [Header("Gun Animations")]
    [SerializeField] Animator _muzzleFlashAnimator;

    public enum FireMode { SemiAuto, FullAuto, Burst }
    public FireMode fireMode = FireMode.FullAuto;

    private void Start()
    {
        BulletsLeft = MagSize;
        BulletCountText.text = BulletsLeft.ToString();
        CanShoot = true;
        AutoReload = true;
    }

    public void StartFiring()
    {
        //Checking the condition if the player is shooting means the Canshoot is true or is reloading then we are skipping the whole method and again when called if canshoot and isReloading is false then the whole method is executed.
        if (CanShoot || isReloading) return;
        CanShoot = true;

        switch (fireMode)
        {
            case FireMode.FullAuto:
                if (ModeText != null)
                    ModeText.text = "FULL AUTO";
                StartCoroutine(FireAuto());
                break;
            case FireMode.SemiAuto:
                if(ModeText != null)
                    ModeText.text = "SEMI-AUTO";
                ShootOnce();
                CanShoot = false;
                break;
            case FireMode.Burst:
                if (ModeText != null)
                    ModeText.text = "BURST";
                StartCoroutine(FireBurst());
                break;
        }

    }

    public void StopFiring()
    {
        CanShoot = false;
    }

    IEnumerator FireAuto()
    {

        while (CanShoot && BulletsLeft > 0)
        {
            ShootOnce();
            yield return new WaitForSeconds(FullAutoFireRate);
        }

        if (BulletsLeft == 0 && AutoReload)
        {
            yield return Reload();
        }
    }

    IEnumerator FireBurst()
    {
        int shotsFired = 0;
        while (shotsFired < BurstCount && BulletsLeft > 0)
        {
            ShootOnce();
            shotsFired++;
            yield return new WaitForSeconds(BurstFireRate);
        }
        if (BulletsLeft == 0 && AutoReload)
        {
            yield return Reload();
        }

        CanShoot = false;
    }

    void ShootOnce()
    {
        if (BulletsLeft <= 0 || isReloading)
        {
            if (BulletsLeft == 0 && AutoReload && !isReloading)
            {
                StartCoroutine(Reload());
            }
            return;
        }

        Vector3 origin = GunBarrel.position;
        Vector3 mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 TargetDirection = (mousepos - origin).normalized;

        var RayHit = Physics2D.Raycast(origin, TargetDirection, BulletRange, hitLayer);

        Debug.DrawLine(origin, origin + TargetDirection * 100f, Color.magenta, .1f);

        //Instantiating Bullet trails
        var BulletTrail = PoolManager.SpawnObject(bulletTrail, origin, Quaternion.identity, PoolManager.PoolType.GameObjects);
        _muzzleFlashAnimator.SetTrigger("Shoot");

        var trailScript = BulletTrail.GetComponent<BulletTracer>();

        if (RayHit.collider)
        {
            trailScript.initialize(origin, RayHit.point, RayHit);
        }
        else
        {
            var endPosition = origin + TargetDirection * BulletRange;
            trailScript.initialize(origin, endPosition, new RaycastHit2D());
        }
        //-----------------------------
        //Instantiating Bullet Prefab
        //var bulletPre = PoolManager.SpawnObject(bulletPrefab, GunBarrel.position, Quaternion.identity, PoolManager.PoolType.GameObjects);
        //_muzzleFlashAnimator.SetTrigger("Shoot");
        //---------------------------------
        BulletsLeft--;
        BulletCountText.text = BulletsLeft.ToString();
        //Debug.Log("Bullets Decrementing while shooting: " + BulletsLeft);

    }

    public IEnumerator Reload()
    {
        isReloading = true;
        ReloadText.text = "RELOADING";
        yield return new WaitForSeconds(ReloadTime);
        BulletsLeft = MagSize;
        //Debug.Log("Bullets Reloaded: " + BulletsLeft);

        isReloading = false;
        ReloadText.text = " ";
    }
}
