using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialCompletedPanelBinder : MonoBehaviour
{
    [SerializeField] private Button _playAgainButton;
    [SerializeField] private Button _exitToMainButton;

    private void OnEnable()
    {
        if (scene_Manager._instance == null)
        {
            Debug.LogError("scene_Manager is not Found");
            return;
        }

        //Resume Button --
        _playAgainButton.onClick.RemoveAllListeners();
        _playAgainButton.onClick.AddListener(scene_Manager._instance.OnTutorialButtonPressed);

        //Exit to Main --
        _exitToMainButton.onClick.RemoveAllListeners();
        _exitToMainButton.onClick.AddListener(scene_Manager._instance.OnExitButtonPressed);

    }
}
