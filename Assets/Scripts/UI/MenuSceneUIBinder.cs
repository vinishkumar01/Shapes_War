using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuSceneUIBinder : MonoBehaviour
{
    [SerializeField] private Button _survivalButton;
    [SerializeField] private Button _tutorialButton;
    [SerializeField] private Button _optionButton;
    [SerializeField] private Button _returnButton;
    [SerializeField] private Button _quitButton;

    private void Start()
    {
        if (scene_Manager._instance == null)
        {
            Debug.LogError("scene_Manager is null");
            return;
        }

        //survival button
        _survivalButton.onClick.RemoveAllListeners();
        _survivalButton.onClick.AddListener(scene_Manager._instance.OnSurvivalButtonPressed);

        //tutorial button
        _tutorialButton.onClick.RemoveAllListeners();
        _tutorialButton.onClick.AddListener(scene_Manager._instance.OnTutorialButtonPressed);

        //option button
        _optionButton.onClick.RemoveAllListeners();
        _optionButton.onClick.AddListener(scene_Manager._instance.OnOptionButtonPressed);

        //quit Button
        _quitButton.onClick.RemoveAllListeners();
        _quitButton.onClick.AddListener(scene_Manager._instance.OnQuitButtonPressed);

        //Return Button
        _returnButton.onClick.RemoveAllListeners();
        _returnButton.onClick.AddListener(scene_Manager._instance.OnReturnButtonPressed);
    }
}
