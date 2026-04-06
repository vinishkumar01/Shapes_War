using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Linq;

public class scene_Manager : MonoBehaviour
{
    public static scene_Manager _instance;

    public string[] _sceneNames = new string[] { "abandoned City", "UnderGround - Area" };
    public string[] _sceneNamesForMenuPanel = new string[] { "abandoned City", "UnderGround - Area", "Tutorial" };
    private string _loadingSceneName = "Loading Screen";
    public string _menuSceneName = "Menu Scene";
    public string _tutorialSceneName = "Tutorial";

    private GameObject _menuPanel;
    private GameObject _optionPanel;
    private GameObject _gameOverPanel;
    private GameObject _tutorialOverPanel;

    [Header("Seperate Scene Check")]
    public int _abandonedCity = 3;
    public int _underGroundArea = 4;

    // Scenes to exclude
    //private HashSet<string> excludedScenes = new HashSet<string> { "Loading Screen", "Learning"};

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        _instance = this;
    }

    private void Update()
    {
        if(UserInputs.instance._playerInputs.Player.Escape.WasPressedThisFrame())
        {
            Debug.Log("Escape Pressed");
            HandleEscape();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        //Reseting the Time scale on any scene load
        Time.timeScale = 1f;
        //Debug.Log($"Time: {Time.timeScale}");

        Debug.Log("Current Scene" + sceneName);

        //Option Panel-------------------------------
        if (sceneName == _menuSceneName)
        {
            GameObject optionPanel = GameObject.FindGameObjectWithTag("OptionPanel");

            if (optionPanel == null)
            {
                Debug.LogWarning("Option panel not found in the scene");
                _optionPanel = null;
            }
            if (optionPanel != null)
            {
                _optionPanel = optionPanel;
                _optionPanel.SetActive(false);
            }
           
        }

        //Menu Panel-------------------------------
        if (IsGameplayScenesToCheckForMenuPanel(sceneName))
        {
            GameObject panel = GameObject.FindGameObjectWithTag("MenuPanel");

            if (panel != null)
            {
                _menuPanel = panel;
                _menuPanel.SetActive(false);
            }
        }
        else
        {
            Debug.Log("no menu panel in this: " + sceneName);
            _menuPanel = null;
        }

        

        //GameOver Panel-----------------------------
        if(IsGameplayScenes(sceneName))
        {
            GameObject gameOverPanel = GameObject.FindGameObjectWithTag("GameOverPanel");

            if (gameOverPanel != null)
            {
                _gameOverPanel = gameOverPanel;
                _gameOverPanel.SetActive(false);
            }
        }
        else
        {
            Debug.Log("no GameOver panel in this: " + sceneName);
            _gameOverPanel = null;
        }

        //Tutorial Over Panel-----------------------------
        if (sceneName == _tutorialSceneName)
        {
            GameObject tutorialOverPanel = GameObject.FindGameObjectWithTag("TutorialOverPanel");

            if (tutorialOverPanel == null)
            {
                Debug.LogWarning("Tutorialover panel not Found in scene");
                _tutorialOverPanel = null;
                return;
            }

            _tutorialOverPanel = tutorialOverPanel;
            _tutorialOverPanel.SetActive(false);
        }  
        else
        {
            Debug.Log("no tutorialOver panel in this: " + sceneName);
            _tutorialOverPanel = null;
        }
    }

    public void OnSurvivalButtonPressed()
    {
        Debug.Log("Survival Button Pressed");

        int randomIndex = Random.Range(0, _sceneNames.Length);
        string selectedScene = _sceneNames [randomIndex];
        //string selectedScene = sceneDropdown.options[sceneDropdown.value].text;

        Debug.Log("Selected scene: " + selectedScene); // Debug check

        // Save the selected scene name (so the transition scene knows what to load next)
        //PlayerPrefs.SetString("NextScene", selectedScene);
        SceneLoader.NextSceneName = selectedScene;
        SceneManager.LoadScene(_loadingSceneName);
    }

    public void OnTutorialButtonPressed()
    {
        SceneLoader.NextSceneName = _tutorialSceneName;

        // Load transition (black screen) scene
        SceneManager.LoadScene(_loadingSceneName, LoadSceneMode.Single);
    }

    public void OnOptionButtonPressed()
    {
        _optionPanel.SetActive(true);
    }

    public void OnReturnButtonPressed()
    {
        _optionPanel.SetActive(false);
    }

    public void OnQuitButtonPressed()
    {
        #if UNITY_EDITOR 
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    #region Menu Panel Config

    public bool IsGameplayScenesToCheckForMenuPanel(string SceneIndex)
    {
        foreach (string index in _sceneNamesForMenuPanel)
        {
            if (index == SceneIndex)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsGameplayScenes(string SceneIndex)
    {
        foreach(string index in _sceneNames)
        {
            if(index == SceneIndex)
            {
                return true;
            }
        }

        return false ;
    }

    public bool IsTutorialScene(string SceneIndex)
    {
        if(_tutorialSceneName == SceneIndex)
        {
            return true;
        }

        return false;
    }

    private void HandleEscape()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if(IsGameplayScenesToCheckForMenuPanel(currentSceneName))
        {
            AcessPauseMenu();
        }
    }

    private void AcessPauseMenu()
    {
        if(_menuPanel == null)
        {
            return;
        }

        bool isActive = _menuPanel.activeInHierarchy;

        _menuPanel.SetActive(!isActive);

       Time.timeScale = isActive ? 1f : 0f;

        //Disable the player Inputs too
        GameState.CanPlayerControl = isActive;
    }

    public void OnResumeButtonPressed()
    {
        bool isActive = _menuPanel.activeInHierarchy;

        _menuPanel.SetActive(!isActive);

        Time.timeScale = isActive ? 1f : 0f;

        //Disable the player Inputs too
        GameState.CanPlayerControl = isActive;
    }

    public void OnExitButtonPressed()
    {
        Debug.Log("Exit to Main button has been pressed");

        Time.timeScale = 1f;

        SceneLoader.NextSceneName = _menuSceneName;

        // Load transition (black screen) scene
        SceneManager.LoadScene(_loadingSceneName, LoadSceneMode.Single);
    }
    #endregion

    public void OpenGameOverPanel()
    {
        Debug.Log("Trying to open GameOver panel: " + _gameOverPanel);

        if (_gameOverPanel == null)
        {
            _gameOverPanel = GameObject.FindGameObjectWithTag("GameOverPanel");
            Debug.Log("gameOver panel was null: re-found" + _gameOverPanel);
        }

       if(_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);
        }
       else
        {
            Debug.LogError("gameOver panel is still null");
        }
    }

    public void OpenTutorialOverPanel()
    {
        _tutorialOverPanel.SetActive(true);
    }

    public void OnDialougeToggleChanged(bool isOn)
    {
        GameState.DialougeEnabled = isOn;
        PlayerPrefs.SetInt("DialogueEnabled", isOn ? 1 : 0);
    }


}
