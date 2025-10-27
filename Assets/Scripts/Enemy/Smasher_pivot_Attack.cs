using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smasher_pivot_Attack : MonoBehaviour
{
    [Header("Reference")]
    Smasher_Test_Script smasherScript;

    [Header("Rotate Configs")]
    [SerializeField] float slamAngle = -90f;
    [SerializeField] float slamSpeed = 300f;
    [SerializeField] float returnSpeed = 150f;
    [SerializeField] float pauseBeforeReturn = 0.5f;
    [SerializeField] float coolDown = 2f;

    [SerializeField] bool isSlaming = false;
    Quaternion initialRotation;
    Quaternion targetRotation;


    private void Start()
    {
        smasherScript = GetComponentInChildren<Smasher_Test_Script>();

        initialRotation = transform.rotation;
        targetRotation = Quaternion.Euler(0,0,slamAngle); ;
    }

    private void Update()
    {
        if(smasherScript.maintainDistance && !isSlaming)
        {
            StartCoroutine(Slam());
        }

        //if(isSlaming)
        //{
        //    smasherScript.platformBelow = false;
        //    smasherScript.platformside = false;
        //}
        //else
        //{
        //    smasherScript.platformBelow = false;
        //}
    }

    IEnumerator Slam()
    {
        isSlaming = true;

        while(Quaternion.Angle(transform.rotation, targetRotation) > 0.5f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, slamSpeed * Time.deltaTime);
            yield return null;
        }

        //pause briefly at the bottom
        yield return new WaitForSeconds(pauseBeforeReturn);

        //Rotate BackUp 
        while(Quaternion.Angle(transform.rotation, initialRotation) > 0.5f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, initialRotation, returnSpeed * Time.deltaTime);
            yield return null;
        }

        //CoolDown before the next slam
        yield return new WaitForSeconds(coolDown);

        isSlaming = false ;
    }

}
