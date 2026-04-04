using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanelBinder : MonoBehaviour
{
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _exitToMainButton;
    [SerializeField] private Button _quitButton;

    private void OnEnable()
    {
        if(scene_Manager._instance == null)
        {
            Debug.LogError("scene_Manager is not Found");
            return;
        }

        //Resume Button --
        _resumeButton.onClick.RemoveAllListeners();
        _resumeButton.onClick.AddListener(scene_Manager._instance.OnResumeButtonPressed);

        //Exit to Main --
        _exitToMainButton.onClick.RemoveAllListeners();
        _exitToMainButton.onClick.AddListener(scene_Manager._instance.OnExitButtonPressed);

        //QuitButton --
        _quitButton.onClick.RemoveAllListeners();
        _quitButton.onClick.AddListener(scene_Manager._instance.OnQuitButtonPressed);
    }
}
