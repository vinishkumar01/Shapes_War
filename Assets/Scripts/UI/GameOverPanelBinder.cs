using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanelBinder : MonoBehaviour
{
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _exitToMainButton;

    private void OnEnable()
    {
        if (scene_Manager._instance == null)
        {
            Debug.LogError("scene_Manager is not Found");
            return;
        }

        //Resume Button --
        _retryButton.onClick.RemoveAllListeners();
        _retryButton.onClick.AddListener(scene_Manager._instance.OnSurvivalButtonPressed);

        //Exit to Main --
        _exitToMainButton.onClick.RemoveAllListeners();
        _exitToMainButton.onClick.AddListener(scene_Manager._instance.OnExitButtonPressed);

    }
}
