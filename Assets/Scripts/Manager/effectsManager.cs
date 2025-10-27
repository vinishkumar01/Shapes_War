using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class effectsManager : MonoBehaviour
{
    private static effectsManager instance;

    private void Awake()
    {
        if(instance == null)
            instance = this;
    }
   
    public static void RunCoroutine(IEnumerator routine)
    {
        if(instance != null)
        {
            instance.StartCoroutine(routine);
        }
    }
}
