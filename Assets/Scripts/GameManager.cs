using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    [Tooltip("The singleton instance")]
    public static GameManager Instance; // singleton
    private PlayerInput m_PlayerInput;
    private GameObject playerReference = null;
    private Camera mainCamera;

    // Pause settings
    GameObject HUD;
    GameObject[] pauseObjects;
    GameObject[] crosshairObjects;
    GameObject[] controlsObjects;
    GameObject[] generalObjects;
    GameObject[] settingsObjects;
    GameObject[] graphicsObjects;
    GameObject[] audioObjects;
    GameObject[] mainMenuObjects;
    public bool enablePause;
    public bool isPaused = false;
    private float pauseWait = 0;
    private float pauseWaitMax = 0.1f;
    public float lookSpeedMouseX = 1.0f;
    public float lookSpeedMouseY = 1.0f;
    public float lookSpeedControllerX = 1.0f;
    public float lookSpeedControllerY = 1.0f;
    public bool glovesIsHold = false;
    public bool UseBootFade = false;
    public bool highContrast = false;
    public float MENU_ALPHA_DEFAULT = .88f;
    public float MENU_ALPHA_HIGH_CONTRAST = 1f;

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
        generalObjects = GameObject.FindGameObjectsWithTag("General");
        settingsObjects = GameObject.FindGameObjectsWithTag("Settings");
        graphicsObjects = GameObject.FindGameObjectsWithTag("Graphics");
        audioObjects = GameObject.FindGameObjectsWithTag("Audio");
        mainMenuObjects = GameObject.FindGameObjectsWithTag("Main_Menu");
        playerReference = GameObject.Find("Player");
        HUD = GameObject.FindGameObjectWithTag("HUD");

        ES = GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>();

        SM = SubtitleManager.getSubtitleManager();
        SM.menuParent = GameObject.FindGameObjectsWithTag("Menu")[0];
        SM.defaultParent = GameObject.FindGameObjectsWithTag("Subtitles")[0];
        SM.canvas = GameObject.Find("Canvas");
        SM.moveSubtitlesToDefault();

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

        mainCamera = Camera.main;

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
    /// Changes the resolution the game runs at
    /// </summary>
    public void ChangeResolution(int width, int height)
    {
        Screen.SetResolution(width, height, Screen.fullScreen);
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

    public void SetFOV(float fov) {
        mainCamera.fieldOfView = fov;
    }

    /// <summary>
    /// Scene Function: Progress to the next scene numerically
    /// </summary>
    public void nextScene() {
        //Debug.Log("GO");
        int nextIndex = currScene.buildIndex+1;
        if(nextIndex != 0) { enablePause = true;} 
        else { enablePause = false;}
        isPaused = false;
        DisableCamera();
        SM.unparent();
        Destroy(GameObject.FindGameObjectWithTag("MusicManager"));
        
        SceneManager.LoadScene(nextIndex, LoadSceneMode.Single);

    }

    /// <summary>
    /// Scene Function: Progress to the previous scene numerically
    /// </summary>
    public void prevScene() {
        int nextIndex = currScene.buildIndex-1;
        if(nextIndex != 0) { enablePause = true;} 
        else { enablePause = false;}
        isPaused = false;
        DisableCamera();
        SM.unparent();
        SceneManager.LoadScene(nextIndex, LoadSceneMode.Single);
    }

    /// <summary>
    /// Scene Function: Progress to the previous scene numerically
    /// </summary>
    public void reloadScene() {
        playerReference = null;
        isPaused = false;
        DisableCamera();
        SM.unparent();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    /// <summary>
    /// Scene Function: Jump to the main menu scene
    /// </summary>
    public void mainMenu() {
        enablePause = false;
        isPaused = false;
        DisableCamera();
        SM.clearQueue();
        SM.unparent();
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
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            AudioListener.pause = false;
            showPauseMenu(); // This is a hack, I labelled the epilepsy warning with "showOnPause" and simply have the pause menu deactivated in the main menu
            //showSettingsMenu();
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
            AdjustChildUIValues(g);
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
            AdjustChildUIValues(g);
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
            AdjustChildUIValues(g);
            
        }
        GameObject.Find("Fullscreen_Button").GetComponent<Toggle>().SetIsOnWithoutNotify(Screen.fullScreen);
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
            AdjustChildUIValues(g);
        }
        EventSystem.current.SetSelectedGameObject(controlsObjects[0].transform.Find("Start_Button").gameObject);
    }

    /// <summary>
    /// Menu Function: display the subtitles options menu
    /// </summary>   
    public void showGeneralMenu()
    {
        hider();
        foreach(GameObject g in generalObjects)
        {
            g.SetActive(true);
            AdjustChildUIValues(g);
        }
        EventSystem.current.SetSelectedGameObject(generalObjects[0].transform.Find("Start_Button").gameObject);
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
            AdjustChildUIValues(g);
        }
        EventSystem.current.SetSelectedGameObject(audioObjects[0].transform.Find("Start_Button").gameObject);
    }

    /// <summary>
    /// Menu Function: Show the Main Menu
    /// </summary>
    public void showMainMenu() 
    {
        hider();
        foreach(GameObject g in mainMenuObjects)
        {
            g.SetActive(true);
            AdjustChildUIValues(g);
        }
        EventSystem.current.SetSelectedGameObject(mainMenuObjects[0].transform.Find("Start_Button").gameObject);
    }

    /// <summary>
    /// Mend Function: Toggle the opacity of the UI
    /// </summary>
    public void toggleHighContrast()
    {
        highContrast = !highContrast;
        AdjustChildUIValues(HUD);
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
        foreach(GameObject g in generalObjects) 
        {
            g.SetActive(false);
        }
        foreach(GameObject g in audioObjects) 
        {
            g.SetActive(false);
        }                   
        foreach(GameObject g in mainMenuObjects) 
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

    /// <summary>
    /// Given a gameobject, set the opacity of the images with "Background" tags to the currently set value
    /// </summary>
    private void AdjustChildUIValues(GameObject g) {
        Image[] menu_images = g.GetComponentsInChildren<Image>();
        foreach(Image image in menu_images){
            if(image.gameObject.CompareTag("Background")){
                Color temp = image.color;
                if(highContrast ){
                    temp.a = MENU_ALPHA_HIGH_CONTRAST; 
                }else {
                    temp.a = MENU_ALPHA_DEFAULT; 
                }
                image.color = temp;
            }
        }
    }

    public string getActionName(string name)
    {
        return m_PlayerInput.actions[name].GetBindingDisplayString();
    }
}

