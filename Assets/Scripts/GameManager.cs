using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

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
    GameObject[] menuObjects;
    public bool enablePause;
    public bool isPaused = false;
    private float pauseWait = 0;
    private float pauseWaitMax = 0.1f;
    public float lookSpeedX = 1.0f;
    public float lookSpeedY = 1.0f;
    public bool glovesIsHold = false;
    public bool UseBootFade = false;

    // Scene changing
    public Scene currScene;
    public int scenenum = 77;
    private bool isSubtitles = false;
    private SubtitleManager SM;
    private EventSystem ES;


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
        menuObjects = GameObject.FindGameObjectsWithTag("Menu");
        playerReference = GameObject.Find("Player");

        ES = GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>();

        //Debug.Log(ES);

        hidePauseMenu();
        updatePauseState();
    }

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

    // Start is called before the first frame update
    void Start() {
        // track the player
        playerReference = GameObject.Find("Player");
        SM = SubtitleManager.getSubtitleManager();

        //SceneManager.sceneLoaded += onSceneLoad;
        currScene = SceneManager.GetActiveScene();
        scenenum = currScene.buildIndex;
        //pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
        //toHideObjects = GameObject.FindGameObjectsWithTag("HideOnPause");
        SM = SubtitleManager.getSubtitleManager();
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
    /// Stops the camera from rendering anything else
    /// Used to prevent visual artifacts on reloading a scene
    /// </summary>
    private void DisableCamera()
    {
        /*
        Camera.main.cullingMask = 0;
        Camera.main.clearFlags = CameraClearFlags.Nothing;
        */
    }

    /// <summary>
    /// Scene Function: Progress to the next scene numerically
    /// </summary>
    public void nextScene() {
        //Debug.Log("GO");
        int nextIndex = currScene.buildIndex+1;
        if(nextIndex != 0) { enablePause = true;} 
        else { enablePause = false;}
        DisableCamera();
        SceneManager.LoadScene(nextIndex, LoadSceneMode.Single);

    }

    /// <summary>
    /// Scene Function: Progress to the previous scene numerically
    /// </summary>
    public void prevScene() {
        int nextIndex = currScene.buildIndex-1;
        if(nextIndex != 0) { enablePause = true;} 
        else { enablePause = false;}
        DisableCamera();
        SceneManager.LoadScene(nextIndex, LoadSceneMode.Single);
    }

    /// <summary>
    /// Scene Function: Progress to the previous scene numerically
    /// </summary>
    public void reloadScene() {
        playerReference = null;
        DisableCamera();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    /// <summary>
    /// Scene Function: Jump to the main menu scene
    /// </summary>
    public void mainMenu() {
        enablePause = false;
        DisableCamera();
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
    /// Menu Function: Toggle fade effect when using the boots
    /// </summary>
    public void toggleFade() {
        UseBootFade = !UseBootFade;
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
            AudioListener.pause = false;
            hidePauseMenu();
            showSettingsMenu();
        }
        else if(isPaused)
        {
            Time.timeScale = 0; 
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            AudioListener.pause = true;
            showPauseMenu();
        } 
        else 
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            AudioListener.pause = false;
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
        EventSystem.current.SetSelectedGameObject(pauseObjects[0].transform.Find("Start_Button").gameObject);
        
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
        foreach(GameObject g in menuObjects)
        {
            g.SetActive(true);
        }
        if (isSubtitles) {
          isSubtitles = !isSubtitles;
          SM.removeSettingsSubtitle();
        }
        EventSystem.current.SetSelectedGameObject(settingsObjects[0].transform.Find("Start_Button").gameObject);
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
        EventSystem.current.SetSelectedGameObject(graphicsObjects[0].transform.Find("Start_Button").gameObject);
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
        EventSystem.current.SetSelectedGameObject(controlsObjects[0].transform.Find("Start_Button").gameObject);
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
        EventSystem.current.SetSelectedGameObject(subtitleObjects[0].transform.Find("Start_Button").gameObject);
        SM.moveSubtitlesForMenu();
        SM.QueueSubtitle(new SubtitleData("This is an example subtitle", 100000, 0.1f));
        isSubtitles = !isSubtitles;
        //Debug.Log(EventSystem.current);
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
        EventSystem.current.SetSelectedGameObject(audioObjects[0].transform.Find("Start_Button").gameObject);
    }
    
    public void hider()
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

    public void hideMenuObjects(){
        foreach(GameObject g in menuObjects) 
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
