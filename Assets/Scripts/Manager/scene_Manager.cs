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

    public int[] _sceneIndices = new int[] { 3, 4 };

    private int _tutorialSceneIndex = 2;

    [SerializeField] private GameObject _menuPanel;

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
        int sceneIndex = scene.buildIndex;

        if (!IsGameplayScenes(sceneIndex))
        {
            _menuPanel = null;
            return;
        }

        GameObject panel = GameObject.FindGameObjectWithTag("MenuPanel");

        if (panel == null)
        {
            Debug.LogWarning("MenuPanel not Found in scene");
            _menuPanel = null;
            return;
        }

        _menuPanel = panel;
        _menuPanel.SetActive(false);
    }

    public void OnSurvivalButtonPressed()
    {
        int randomIndex = Random.Range(0, _sceneIndices.Length);
        int selectedScene = _sceneIndices[randomIndex];
        //string selectedScene = sceneDropdown.options[sceneDropdown.value].text;

        Debug.Log("Selected scene: " + selectedScene); // Debug check

        // Save the selected scene name (so the transition scene knows what to load next)
        //PlayerPrefs.SetString("NextScene", selectedScene);
        PlayerPrefs.SetInt("NextScene", selectedScene);
        PlayerPrefs.Save();

        // Load transition (black screen) scene
        SceneManager.LoadScene("Loading Screen");
    }

    #region Menu Panel Config

    private bool IsGameplayScenes(int SceneIndex)
    {
        foreach(int index in _sceneIndices)
        {
            if(index == SceneIndex)
            {
                return true;
            }
        }

        if(_tutorialSceneIndex == SceneIndex)
        {
            return true ;
        }

        return false ;
    }

    private void HandleEscape()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if(IsGameplayScenes(currentSceneIndex))
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
    }

    #endregion



}
