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
    [SerializeField] private Toggle _dialogueSystem;
    [SerializeField] private Button _resetHighScoreButton;

    private void Start()
    {
        if (scene_Manager._instance == null)
        {
            Debug.LogError("scene_Manager is null");
            return;
        }

        //saving the dialogue playerPrefs
        int saved = PlayerPrefs.GetInt("DialogueEnabled", 1);
        bool isOn = saved == 1;

        GameState.DialougeEnabled = isOn;
        _dialogueSystem.isOn = isOn;


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

        //Reset HighScore Button
        _resetHighScoreButton.onClick.RemoveAllListeners();
        _resetHighScoreButton.onClick.AddListener(() =>
        {
            PlayerPrefs.DeleteKey("HighScore");
            PlayerPrefs.Save();

            UIManager.InvokeScoreUpdate(0, 0);
        });

        //toggle
        _dialogueSystem.onValueChanged.RemoveAllListeners();
        _dialogueSystem.onValueChanged.AddListener(scene_Manager._instance.OnDialougeToggleChanged);
    }
}
