using System.Collections;
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
    GameObject[] toHideObjects;
    GameObject[] controlsObjects;
    public bool enablePause;
    public bool isPaused = false;
    private float pauseWait = 0;
    private float pauseWaitMax = 0.1f;
    public float lookSpeedX = 4.0f;
    public float lookSpeedY = 4.0f;
    public bool glovesIsHold = false;

    // Scene changing
    public Scene currScene;
    public int scenenum = 77;

    public static GameManager getGameManager() {
        return Instance;
    }

    public void setPlayerInput(PlayerInput playerInput) {
        m_PlayerInput = playerInput;
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
        toHideObjects = GameObject.FindGameObjectsWithTag("HideOnPause");
        controlsObjects = GameObject.FindGameObjectsWithTag("Controls");
        playerReference = GameObject.Find("Player");
        hidePaused();
        updatePauseState();
    }

    // Start is called before the first frame update
    void Awake()
    {

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
    void FixedUpdate()
    {   
        if (pauseWait > 0)
        {
            pauseWait -= Time.unscaledDeltaTime;
        }
        if (m_PlayerInput.actions["Menu"].ReadValue<float>() == 1)
        {
            // TODO: should unpause if already paused
            if (enablePause && pauseWait <= 0) {
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
            hidePaused();
        }
        else if(isPaused)
        {
            Time.timeScale = 0; 
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            showPaused();
        } 
        else 
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            hidePaused();
        }
    }

    /// <summary>
    /// Menu Function: show objects with PauseObject tag
    /// </summary>
    public void showPaused() {

        foreach(GameObject g in pauseObjects) {
            g.SetActive(true);
        }
        foreach(GameObject g in toHideObjects) {
            g.SetActive(false);
        }
        foreach(GameObject g in controlsObjects) {
            g.SetActive(false);
        }
    }

    /// <summary>
    /// Menu Function: hide objects with PauseObject tag
    /// </summary>
    public void hidePaused() {
        foreach(GameObject g in pauseObjects) {
            g.SetActive(false);
        }
        foreach(GameObject g in toHideObjects) {
            g.SetActive(true);
        }
        foreach(GameObject g in controlsObjects) {
            g.SetActive(false);
        }
    }

    /// <summary>
    /// Menu Function: display the control options menu
    /// </summary>
    public void showControls() 
    {
      foreach(GameObject g in controlsObjects)
      {
        g.SetActive(true);
      }
      foreach(GameObject g in pauseObjects)
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
