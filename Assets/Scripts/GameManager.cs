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
    GameObject Menu;
    GameObject pauseMenu;
    GameObject[] crosshairObjects;
    GameObject controlsMenu;
    GameObject generalMenu;
    GameObject settingsMenu;
    GameObject graphicsMenu;
    GameObject audioMenu;
    GameObject mainMenuObject;
    GameObject credits;
    GameObject warning;
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
    private float camFOV = 60;
    private GameObject Blackout;

    private bool cameFromOtherScene = false;

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

        GameObject Canvas = GameObject.Find("Canvas");
        if(Menu == null){
            Menu = GameObject.FindGameObjectWithTag("Menu");
        }
        else if(GameObject.FindGameObjectsWithTag("Menu").Length > 1){
            foreach(Transform child in Canvas.GetComponent<Transform>()){
                if(child.tag == "Menu"){
                    Destroy(child.gameObject);
                }
            }
        }
        Transform MenuTransform = Menu.GetComponent<Transform>();
        MenuTransform.SetParent(Canvas.GetComponent<Transform>());

        //Debug.Log(scenenum);
        Time.timeScale = 1;
        crosshairObjects = GameObject.FindGameObjectsWithTag("Crosshair");
        playerReference = GameObject.Find("Player");
        HUD = GameObject.FindGameObjectWithTag("HUD");
        mainMenuObject = GameObject.FindGameObjectWithTag("Main_Menu");
        credits = GameObject.FindGameObjectWithTag("Credits");
        warning = GameObject.FindGameObjectWithTag("Warning");
        Blackout = GameObject.FindGameObjectWithTag("Blackout");
        mainCamera = Camera.main;
        mainCamera.fieldOfView = camFOV;

        ES = GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>();

        SM = SubtitleManager.getSubtitleManager();
        foreach(Transform child in Menu.GetComponentsInChildren<Transform>()){
            if(child.tag == "ShowOnPause"){
                pauseMenu = child.gameObject;
                // Debug.Log(pauseMenu);
            }
            if(child.tag == "Controls")
                controlsMenu = child.gameObject;
            if(child.tag == "General")
                generalMenu = child.gameObject;
            if(child.tag == "Settings")
                settingsMenu = child.gameObject;
            if(child.tag == "Graphics")
                graphicsMenu = child.gameObject;
            if(child.tag == "Audio")
                audioMenu = child.gameObject;
            if(child.tag == "Menu_Subtitle_Pos")
                SM.menuParent = child.gameObject;
        }
        if(SceneManager.GetActiveScene().buildIndex == 0){
            showMainMenu();
        }

        SM.defaultParent = GameObject.FindGameObjectWithTag("Subtitles");

        SM.canvas = Canvas;
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


        //SceneManager.sceneLoaded += onSceneLoad;
        currScene = SceneManager.GetActiveScene();
        scenenum = currScene.buildIndex;
        //pauseMenu = GameObject.FindGameObjectsWithTag("ShowOnPause");
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
    public void ChangeResolution(Resolution res)
    {
        Screen.SetResolution(res.width, res.height, Screen.fullScreen, res.refreshRate);
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
        camFOV = fov;
        mainCamera.fieldOfView = fov;
    }

    public void StartGame() {
        hider();
        Blackout.SetActive(true);
        Blackout.GetComponent<Image>().color = Color.black;
        nextScene();

    }
    /// <summary>
    /// Scene Function: Progress to the next scene numerically
    /// </summary>
    public void nextScene() {
        cameFromOtherScene = true;
        //Debug.Log("LEAVING " + currScene.buildIndex);
        int nextIndex = currScene.buildIndex+1;
        if(nextIndex != 0) { enablePause = true;} 
        else { enablePause = false;}
        isPaused = false;
        DisableCamera();
        SM.unparent();
        Menu.GetComponent<Transform>().parent = null;
        DontDestroyOnLoad(Menu);
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
        Menu.GetComponent<Transform>().parent = null;
        DontDestroyOnLoad(Menu);
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
        Menu.GetComponent<Transform>().parent = null;
        DontDestroyOnLoad(Menu);
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
        Menu.GetComponent<Transform>().parent = null;
        DontDestroyOnLoad(Menu);
        Destroy(GameObject.FindGameObjectWithTag("MusicManager"));
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
            if(cameFromOtherScene) {
                showMainMenu();
            }else {
                showWarning();
            }
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

    public void settingsBack(){
        if(enablePause){
            showPauseMenu();
        }else {
            showMainMenu();
        }
    }

    /// <summary>
    /// Menu Function: show objects with PauseObject tag
    /// </summary>
    public void showPauseMenu()
    {
        hider();
        showMenu(pauseMenu);
        EventSystem.current.SetSelectedGameObject(pauseMenu.transform.Find("Start_Button").gameObject);
    }
    public void showWarning(){
        hider();
        showMenu(warning);
        EventSystem.current.SetSelectedGameObject(warning.transform.Find("Start_Button").gameObject);
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
        showMenu(settingsMenu);
        if (isSubtitles) {
          isSubtitles = !isSubtitles;
          SM.removeSettingsSubtitle();
        }
        EventSystem.current.SetSelectedGameObject(settingsMenu.transform.Find("Start_Button").gameObject);
    }

    /// <summary>
    /// Menu Function: display the graphics options menu
    /// </summary>
    public void showGraphicsMenu() 
    {
        hider();
        showMenu(graphicsMenu);
        GameObject.Find("Fullscreen_Button").GetComponent<Toggle>().SetIsOnWithoutNotify(Screen.fullScreen);
        EventSystem.current.SetSelectedGameObject(graphicsMenu.transform.Find("Start_Button").gameObject);
    }

    /// <summary>
    /// Menu Function: display the control options menu
    /// </summary>
    public void showControlsMenu() 
    {
        hider();
        showMenu(controlsMenu);
        EventSystem.current.SetSelectedGameObject(controlsMenu.transform.Find("Start_Button").gameObject);
    }

    /// <summary>
    /// Menu Function: display the subtitles options menu
    /// </summary>   
    public void showGeneralMenu()
    {
        hider();
        showMenu(generalMenu);
        EventSystem.current.SetSelectedGameObject(generalMenu.transform.Find("Start_Button").gameObject);
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
        showMenu(audioMenu);
        EventSystem.current.SetSelectedGameObject(audioMenu.transform.Find("Start_Button").gameObject);
    }

    /// <summary>
    /// Menu Function: Show the Main Menu
    /// </summary>
    public void showMainMenu() 
    {
        hider();
        showMenu(mainMenuObject);
        EventSystem.current.SetSelectedGameObject(mainMenuObject.transform.Find("Start_Button").gameObject);
    }

    /// <summary>
    /// Menu Function: Show the Credits
    /// </summary>
    public void showCredits() 
    {
        hider();
        showMenu(credits);
        EventSystem.current.SetSelectedGameObject(credits.transform.Find("Start_Button").gameObject);
    }



    /// <summary>
    /// Mend Function: Toggle the opacity of the UI
    /// </summary>
    public void toggleHighContrast()
    {
        highContrast = !highContrast;
        AdjustChildUIValues(HUD);
    }

    private void showMenu(GameObject menu){
        if(menu!=null) {
            menu.SetActive(true);
            AdjustChildUIValues(menu);
        }
       
    }
    private void hideMenu(GameObject menu) {
        if(menu != null){
            menu.SetActive(false);
        }
    }
    
    public void hider()
    {
        hideMenu(pauseMenu);
        hideMenu(settingsMenu);
        hideMenu(graphicsMenu);
        hideMenu(controlsMenu);
        hideMenu(generalMenu);
        hideMenu(audioMenu);
        hideMenu(mainMenuObject);
        hideMenu(credits);
        hideMenu(warning);
        foreach(GameObject g in crosshairObjects) 
        {
            if(g != null){
                g.SetActive(false);
            }
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

