﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Tooltip("The singleton instance")]
    public static GameManager Instance; // singleton
    private PlayerInput m_PlayerInput;
    private GameObject playerReference = null;

    // Pause settings
    GameObject[] pauseObjects;
    GameObject[] crosshairObjects;
    GameObject[] controlsObjects;
    GameObject[] subtitleObjects;
    GameObject[] settingsObjects;
    GameObject[] graphicsObjects;
    GameObject[] audioObjects;
    public bool enablePause;
    public bool isPaused = false;
    private float pauseWait = 0;
    private float pauseWaitMax = 0.1f;
    public float lookSpeedX = 1.0f;
    public float lookSpeedY = 1.0f;
    public bool glovesIsHold = false;

    // Scene changing
    public Scene currScene;
    public int scenenum = 77;
    private bool isSubtitles = false;
    private SubtitleManager SM;

    public static GameManager getGameManager() {
        return Instance;
    }

    public PlayerInput getPlayerInput()
    {
      return m_PlayerInput;
    }

   // called first
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SM = SubtitleManager.getSubtitleManager();
    }

    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("OnSceneLoaded: " + scene.name);
        //Debug.Log(mode);
        currScene = SceneManager.GetActiveScene();
        scenenum = currScene.buildIndex;
        //Debug.Log(scenenum);
        Time.timeScale = 1;
        pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
        crosshairObjects = GameObject.FindGameObjectsWithTag("Crosshair");
        controlsObjects = GameObject.FindGameObjectsWithTag("Controls");
        subtitleObjects = GameObject.FindGameObjectsWithTag("Subtitles");
        settingsObjects = GameObject.FindGameObjectsWithTag("Settings");
        graphicsObjects = GameObject.FindGameObjectsWithTag("Graphics");
        audioObjects = GameObject.FindGameObjectsWithTag("Audio");
        playerReference = GameObject.Find("Player");

        hidePauseMenu();
        updatePauseState();
    }

    // Start is called before the first frame update
    void Awake()
    {

        m_PlayerInput = GetComponent<PlayerInput>();

        // singleton insurance
        if(Instance == null) {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if(Instance != this) {
            Destroy(gameObject);
        }
        
    }
    void Start() {
        // track the player
        playerReference = GameObject.Find("Player");

        //SceneManager.sceneLoaded += onSceneLoad;
        currScene = SceneManager.GetActiveScene();
        scenenum = currScene.buildIndex;
        //pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
        //toHideObjects = GameObject.FindGameObjectsWithTag("HideOnPause");
        updatePauseState();
    }

    // called when the game is terminated
    void OnDisable()
    {
        //Debug.Log("OnDisable");
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //controls.Disable();
    }

    // Update is called once per frame
    void Update()
    {   

        if (m_PlayerInput != null && m_PlayerInput.actions["Menu"].triggered )
        {
            if (enablePause) {
                switchPause();
            }
        }
    }

    /// <summary>
    /// Scene Function: Progress to the next scene numerically
    /// </summary>
    public void nextScene() {
        //Debug.Log("GO");
        int nextIndex = currScene.buildIndex+1;
        if(nextIndex != 0) { enablePause = true;} 
        else { enablePause = false;}
        SceneManager.LoadScene(nextIndex, LoadSceneMode.Single);

    }

    /// <summary>
    /// Scene Function: Progress to the previous scene numerically
    /// </summary>
    public void prevScene() {
        int nextIndex = currScene.buildIndex-1;
        if(nextIndex != 0) { enablePause = true;} 
        else { enablePause = false;}
        SceneManager.LoadScene(nextIndex, LoadSceneMode.Single);
    }

    /// <summary>
    /// Scene Function: Jump to the main menu scene
    /// </summary>
    public void mainMenu() {
        Debug.Log("hello");
        enablePause = false;
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    /// <summary>
    /// Menu Function: Show or hide pause menu elements
    /// </summary>
    public void switchPause() {
        isPaused = !isPaused;
        pauseWait = pauseWaitMax;
        updatePauseState();
    }

    /// <summary>
    /// Show whether the game is paused
    /// </summary>
    private void updatePauseState()
    {
        if (!enablePause)
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            hidePauseMenu();
        }
        else if(isPaused)
        {
            Time.timeScale = 0; 
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            showPauseMenu();
        } 
        else 
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            hidePauseMenu();
        }
    }

    /// <summary>
    /// Menu Function: show objects with PauseObject tag
    /// </summary>
    public void showPauseMenu()
    {
        hider();
        foreach(GameObject g in pauseObjects) 
        {
            g.SetActive(true);
        }
    }

    /// <summary>
    /// Menu Function: hide objects with crosshairObject tag
    /// </summary>
    public void hidePauseMenu()
    {
        hider();
        foreach(GameObject g in crosshairObjects) 
        {
            g.SetActive(true);
        }
    }

    /// <summary>
    /// Menu Function: display the settings menu
    /// </summary>
    public void showSettingsMenu() 
    {
        hider();
        foreach(GameObject g in settingsObjects)
        {
            g.SetActive(true);
        }
        if (isSubtitles) {
          isSubtitles = !isSubtitles;
          SM.removeSettingsSubtitle();
        }
    }

    /// <summary>
    /// Menu Function: display the graphics options menu
    /// </summary>
    public void showGraphicsMenu() 
    {
        hider();
        foreach(GameObject g in graphicsObjects)
        {
            g.SetActive(true);
        }
    }

    /// <summary>
    /// Menu Function: display the control options menu
    /// </summary>
    public void showControlsMenu() 
    {
        hider();
        foreach(GameObject g in controlsObjects)
        {
            g.SetActive(true);
        }
    }

    /// <summary>
    /// Menu Function: display the subtitles options menu
    /// </summary>   
    public void showSubtitleMenu()
    {
        hider();
        foreach(GameObject g in subtitleObjects)
        {
            g.SetActive(true);
        }
        SM.moveSubtitlesForMenu();
        SM.QueueSubtitle(new SubtitleData("This is an example subtitle", 100000, 0.1f));
        isSubtitles = !isSubtitles;

    }

    /// <summary>
    /// Menu Function: display the audio options menu
    /// </summary>
    public void showAudioMenu() 
    {
        hider();
        foreach(GameObject g in audioObjects)
        {
            g.SetActive(true);
        }
    }
    
    private void hider()
    {
        foreach(GameObject g in pauseObjects) 
        {
            g.SetActive(false);
        }
        foreach(GameObject g in crosshairObjects) 
        {
            g.SetActive(false);
        }
        foreach(GameObject g in settingsObjects) 
        {
            g.SetActive(false);
        }
        foreach(GameObject g in graphicsObjects) 
        {
            g.SetActive(false);
        }
        foreach(GameObject g in controlsObjects) 
        {
            g.SetActive(false);
        }
        foreach(GameObject g in subtitleObjects) 
        {
            g.SetActive(false);
        }
        foreach(GameObject g in audioObjects) 
        {
            g.SetActive(false);
        }                   
    }

    /// <summary>
    /// Return the currently stored reference to the player, or attempt to find a new one
    /// </summary>
    public GameObject GetPlayer()
    {
        if (playerReference == null)
        {
            playerReference = GameObject.Find("Player");
        }
        return playerReference;
    }
}
