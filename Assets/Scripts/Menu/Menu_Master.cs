using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class ResData
{
    public int width;
    public int height;
    public int refreshRate = 60;
}

public class Menu_Master : MonoBehaviour
{
    GameManager manager;
    SubtitleManager SM;
    [Tooltip("The HUD's bootup controller")]
    public BootupController BC;
    public AudioMixer masterMix;
    public AudioMixer musicMix;
    public AudioMixer sfxMix;
    public AudioMixer uiMix;
    public AudioMixer dialogMix;
    
    [Tooltip("The set of resolution setting game objects")]
    [Header("Resolution Settings")]
    public GameObject[] ResolutionOptions;
    [Tooltip("The set of default resolutions sizes to use if somehow none are supported. Format: ")]
    public ResData[] DefaultResolutions;
    [Tooltip("The set of default resolutions to use")]
    Resolution[] defaultResolutions;
    [Tooltip("The color to use when a resolution is selected")]
    public Color SelectedColor = new Color(255,192,0,255);
    [Tooltip("The list of supported resolutions")]
    List<Resolution> supportedResolutions = new List<Resolution>();
    [Tooltip("The currently-selected resolution ID")]
    int currentResolutionIndex = 0;

    bool invert_x = false;
    bool invert_y = false;
    public GameObject ContinueButton;
    public GameObject BackButton;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameManager.getGameManager();
        SM = SubtitleManager.getSubtitleManager();

        // Initialize resolution settings
        Resolution[] supportedResSet = Screen.resolutions;
        int supportedResCount = supportedResSet.Length;
        int resolutionOptionCount = ResolutionOptions.Length;
        // 0. use default resolutions if none available
        if (supportedResCount <= 0)
        {
            // initialize default resolutions
            defaultResolutions = new Resolution[DefaultResolutions.Length];
            for (int i = 0; i < DefaultResolutions.Length; i++)
            {
                defaultResolutions[i].width = DefaultResolutions[i].width;
                defaultResolutions[i].height = DefaultResolutions[i].height;
                defaultResolutions[i].refreshRate = DefaultResolutions[i].refreshRate;
            }

            supportedResSet = defaultResolutions;
            supportedResCount = supportedResSet.Length;

            //Debug.Log("No supported resolutions detected. Using defaults...");
        }
        // 1. if fewer resolutions available, reduce options
        if (resolutionOptionCount > supportedResCount)
        {
            for (int i = 0; i < ResolutionOptions.Length; i++)
            {
                if (i >= supportedResCount)
                {
                    ResolutionOptions[i].SetActive(false);
                }
            }
            resolutionOptionCount = supportedResCount;
        }
        
        // DEBUG
        // list out suported resolutions
        /*
        Debug.Log("Supported Res Count: " + supportedResCount);
        for (int i = 0; i < supportedResCount; i++)
        {
            Resolution res = supportedResSet[i];
            Debug.Log("Resolution " + i + ": " + res.width + " x " + res.height + " (RR: " + res.refreshRate + ")");
        }
        */

        // 2. take the highest resolution, lowest resolution, and a sampling in between
        float resolutionIndexStep = (supportedResCount-1)/(resolutionOptionCount-1);
        for (int i = 0; i < resolutionOptionCount-1; i++)
        {
            supportedResolutions.Add(supportedResSet[(int) (i*resolutionIndexStep)]);
        }
        supportedResolutions.Add(supportedResSet[supportedResCount-1]);
        supportedResolutions.Reverse();
        // 3. update resolution display options
        for (int i = 0; i < resolutionOptionCount; i++)
        {
            Text optionText = ResolutionOptions[i].GetComponentInChildren<Text>();
            if (optionText != null)
            {
                Resolution optionRes = supportedResolutions[i];
                optionText.text = optionRes.width + " x " + optionRes.height;
            }
        }
        // 4. apply current resolution
        Resolution currentRes = Screen.currentResolution;
        int currentIndex = supportedResolutions.FindIndex(0,supportedResolutions.Count-1, ResolutionMatchLambda(currentRes));
        if (currentIndex >= 0)
        {
            currentResolutionIndex = currentIndex;
        }
        else
        {
            //Debug.Log("Current resolution not in listing! Defaulting to highest...");
        }
        ChangeResolution(currentResolutionIndex);
    }

    /// <summary>
    /// Returns a lambda function that evaluates whether a resolution matches another
    /// </summary>
    public Predicate<Resolution> ResolutionMatchLambda(Resolution compareTo)
    {
        Predicate<Resolution> resMatch = res =>
        {
            return res.width == compareTo.width && res.height == compareTo.height && res.refreshRate == compareTo.refreshRate;
        };
        return resMatch;
    }

    public void NextScene() {
        manager.nextScene();
    }

    public void StartGame() {
        manager.StartGame();
    }

    // SHOWING DIFFERENT MENUS
    /// <summary>
    /// Unpause the game
    /// </summary>
    public void ContinueGame() {
        manager.switchPause();
    }

    /// <summary>
    /// Go to the settings menu
    /// </summary>
    public void ShowSettingsMenu()
    {
        manager.showSettingsMenu();
    }

    /// <summary>
    /// Go to the controls menu
    /// </summary>
    public void ShowControlsMenu()
    {
        manager.showControlsMenu();
    }

    /// <summary>
    /// Go to the graphics menu
    /// </summary>
    public void ShowGraphicsMenu()
    {
        manager.showGraphicsMenu();
    }

    /// <summary>
    /// Go to the audio menu
    /// </summary>
    public void ShowAudioMenu()
    {
        manager.showAudioMenu();
    }

    /// <summary>
    /// Go to the general menu
    /// </summary>
    public void ShowGeneralMenu()
    {
        manager.showGeneralMenu();
    }

    public void SettingsBack(){
        if(SceneManager.GetActiveScene().buildIndex == 0)
        {
            ContinueButton.SetActive(false);
            BackButton.SetActive(true);
        }
        manager.settingsBack();
    }

    /// <summary>
    /// Show the pause menu
    /// Useful for going back to the pause menu from a sub-menu
    /// </summary>
    public void ShowPauseMenu()
    {
      manager.showPauseMenu();
    }

    /// <summary>
    /// Go to the main menu
    /// Used on the main menu to progress to the main menu
    /// </summary>
    public void ShowMainMenu() {
      manager.showMainMenu();
    }

    // PAUSE MENU OPTIONS

    /// <summary>
    /// Reset the current level
    /// </summary>
    public void ResetLevel() {
        manager.reloadScene();
    }

    /// <summary>
    /// Go back to the main menu from one of the game's levels
    /// </summary>
    public void ReturnToMain() {
        manager.mainMenu();
    }

    /// <summary>
    /// Quit the game
    /// </summary>
    public void QuitGame() {
        Application.Quit();
    }

    /// <summary>
    /// Show the credits
    /// </summary>
    public void ShowCredits() {
        manager.showCredits();
    }



    // GENERAL MENU ACTIONS

    /// <summary>
    /// Toggle the slow subtitles setting
    /// </summary>
    public void ToggleSlowSubtitles()
    {
        SM.slowSubtitles();
    }

    /// <summary>
    /// Change the font size of the subtitle
    /// Calling without an argument just applies the current alpha
    /// </summary>
    /// <param name="backAlpha">[Optional] the new alpha (defaults to current)</param>
    public void SetSubtitleAlpha(float backAlpha = -1)
    {
        SM.SetSubtitleAlpha(backAlpha);
    }

    /// <summary>
    /// Change the font size of the subtitle
    /// Calling without an argument just applies the current alpha
    /// </summary>
    /// <param name="fontSize">[Optional] the new alpha (defaults to current)</param>
    public void SetSubtitleSize(float backAlpha = -1)
    {
        SM.SetSubtitleSize(backAlpha);
    }

    // CONTROLS MENU ACTIONS

    /// <summary>
    /// Change the look speed for the mouse's X
    /// </summary>
    public void ChangeLookSpeedMouseX(float value)
    {
        if(invert_x)
        {
          value = value * (-1f);
        }
        manager.lookSpeedMouseX = value;
    }

    /// <summary>
    /// Change the look speed for the mouse's Y
    /// </summary>
    public void ChangeLookSpeedMouseY(float value) 
    {
        if(invert_y)
        {
          value = value * (-1f);
        }
        manager.lookSpeedMouseY = value;
    }

    /// <summary>
    /// Change the look speed for the controller's X
    /// </summary>
    public void ChangeLookSpeedControllerX(float value)
    {
        if(invert_x)
        {
          value = value * (-1f);
        }
        manager.lookSpeedControllerX = value;
    }

    /// <summary>
    /// Change the looks speed for the controller's Y
    /// </summary>
    public void ChangeLookSpeedControllerY(float value) 
    {
        if(invert_y)
        {
          value = value * (-1f);
        }
        manager.lookSpeedControllerY = value;
    }

    /// <summary>
    /// Invert the X looking
    /// </summary>
    public void InvertX()
    {
        invert_x = !invert_x;
        manager.lookSpeedControllerX = manager.lookSpeedControllerX * (-1.0f);  
        manager.lookSpeedMouseX = manager.lookSpeedMouseX * (-1.0f);  
    }

    /// <summary>
    /// Invert the Y looking
    /// </summary>
    public void InvertY()
    {
        invert_y = !invert_y;
        manager.lookSpeedControllerY = manager.lookSpeedControllerY * (-1.0f);  
        manager.lookSpeedMouseY = manager.lookSpeedMouseY * (-1.0f);  
    }

    /// <summary>
    /// Toggle whether or not the player has to hold buttons
    /// </summary>
    public void ToggleHold()
    {
        manager.glovesIsHold = !manager.glovesIsHold;
    }

    // GRAPHICS MENU OPTIONS

    /// <summary>
    /// Applies the appropriate resolution for the supplied enumm value
    /// (unfortunately Unity restrictions force us to use an int instead,
    /// so use the enum above for reference on the actual values)
    /// </summary>
    public void ChangeResolution(int res)
    {
        Resolution newResolution = supportedResolutions[currentResolutionIndex];

        /*
        switch ((SupportedResolutions) res)
        {
            case SupportedResolutions.RES_1920X1080:
                width = 1920;
                height = 1080;
                break;
            case SupportedResolutions.RES_1600X900:
                width = 1600;
                height = 900;
                break;
            case SupportedResolutions.RES_1024X576:
                width = 1024;
                height = 576;
                break;
            case SupportedResolutions.RES_756X432:
                width = 756;
                height = 432;
                break;
            case SupportedResolutions.RES_512X288:
                width = 512;
                height = 288;
                break;
            default:
                break;
        }
        */
        
        // remove highlight from previous resolution
        Text oldText = ResolutionOptions[currentResolutionIndex].GetComponentInChildren<Text>();
        oldText.color = Color.white;

        // load new resolution size, if available
        if (res >= 0 && res < supportedResolutions.Count)
        {
            currentResolutionIndex = res;
            newResolution = supportedResolutions[currentResolutionIndex];
            BC.RecenterPanels(newResolution.width, newResolution.height);
        }

        // update resolution selection
        Text newText = ResolutionOptions[currentResolutionIndex].GetComponentInChildren<Text>();
        newText.color = SelectedColor;

        manager.ChangeResolution(newResolution);
    }

    /// <summary>
    /// Toggle fullscreen on/off
    /// </summary>
    public void ToggleFullscreen() {
        Screen.fullScreen = !Screen.fullScreen;
    }

    /// <summary>
    /// Toggle the boot fading effect on/off
    /// </summary>
    public void ToggleFade() {
        manager.toggleFade();
    }

    /// <summary>
    /// Toggle high contrast UI
    /// </summary>
    public void ToggleHighContrastUI() {
        manager.toggleHighContrast();
    }

    /// <summary>
    /// Change the FOV
    /// Calling without an argument just applies the current FOV
    /// </summary>
    /// <param name="fov">[Optional] the new alpha (defaults to current)</param>
    public void SetFOV(float fov = -1)
    {
        manager.SetFOV(fov);
    }

    // AUDIO MENU OPTIONS

    /// <summary>
    /// Change the master volume
    /// Calling without an argument just applies the current volume
    /// </summary>
    /// <param name="masterVolume">[Optional] the new volume (defaults to current)</param>
    public void SetMasterVolume(float masterVolume = -1)
    {
        AudioListener.volume = masterVolume;

    }

    /// <summary>
    /// Change the environment volume
    /// Calling without an argument just applies the current volume
    /// </summary>
    /// <param name="musicVolume">[Optional] the new volume (defaults to current)</param>
    public void SetMusicVolume(float musicVolume = -1)
    {
        musicMix.SetFloat("musicVolume", musicVolume);
    }

    /// <summary>
    /// Change the sfx volume
    /// Calling without an argument just applies the current volume
    /// </summary>
    /// <param name="sfxVolume">[Optional] the new volume (defaults to current)</param>
    public void SetSfxVolume(float sfxVolume = -1)
    {
        sfxMix.SetFloat("sfxVolume", sfxVolume);
    }

    /// <summary>
    /// Change the ui volume
    /// Calling without an argument just applies the current volume
    /// </summary>
    /// <param name="uiVolume">[Optional] the new volume (defaults to current)</param>
    public void SetUiVolume(float uiVolume = -1)
    {
        uiMix.SetFloat("uiVolume", uiVolume);
    }

    /// <summary>
    /// Change the dialog volume
    /// Calling without an argument just applies the current volume
    /// </summary>
    /// <param name="dialogVolume">[Optional] the new volume (defaults to current)</param>
    public void SetDialogVolume(float dialogVolume = -1)
    {
        dialogMix.SetFloat("dialogVolume", dialogVolume);
    }
}
