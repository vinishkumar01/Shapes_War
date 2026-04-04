using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSecondsRealtime(1f); // wait 2–3 seconds for transition

        string nextScene = SceneLoader.NextSceneName;
        Debug.Log("Loading next scene: " + nextScene);
        if(string.IsNullOrEmpty(nextScene))
        {
            Debug.LogError("NextSceneName is null or empty");
            yield break;
        }

        SceneManager.LoadScene(nextScene);

        
    }
}
