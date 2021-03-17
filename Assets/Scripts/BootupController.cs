using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Animate the opening sequence
/// </summary>

public class BootupController : MonoBehaviour
{
    [Tooltip("Whether the \"closed eyes\" visual starts")]
    public bool StartVisible = false;
    [Tooltip("Whether or not to play the animation")]
    public bool PlayAnimation = false;
    
    // References
    public Canvas UICanvas;
    public Image PanelTop;
    public Image PanelBottom;
    public Image PanelLeft;
    public Image PanelRight;
    private Vector3 topStart;
    private Vector3 botStart;
    private Vector3 leftStart;
    private Vector3 rightStart;
    private float width;
    private float height;

    
    private float animDelay = 0f;
    private float timer = 0;
    private float timerMax = 15f;
    private bool invertAnimation = false; // whether to play the animation in reverse
    
    // Animation parameters
    private float bootupDelayFactor = 0.1f; // portion of bootup anim to wait before starting
    private float pauseTime = 0.05f; // time at which initial vertical opening stops
    private float xSpeed = 1.25f; // horizontal opening speed after initial wait
    private float yResumeTime = 0.8f; // time at which vertical opening resumes
    private float yInitialSpeed = 1f; // Vertical opening speed in first animation phase
    private float yFinalSpeed; // calculated at start. Vertical opening speed in last animation phase

    void Start()
    {
        width = Screen.width;
        height = Screen.height;
        
        Vector2 screenSize = new Vector2(1600,900);
        Vector3 center = new Vector3(width/2f,height/2f,0);
        
        topStart = center - Vector3.up*height/2f;
        botStart = center + Vector3.up*height/2f;
        leftStart = center - Vector3.right*width/2f;
        rightStart = center + Vector3.right*width/2f;

        PanelTop.transform.position = topStart;
        PanelBottom.transform.position = botStart;
        PanelLeft.transform.position = leftStart;
        PanelRight.transform.position = rightStart;
        
        PanelTop.rectTransform.sizeDelta = screenSize;
        PanelBottom.rectTransform.sizeDelta = screenSize;
        PanelLeft.rectTransform.sizeDelta = screenSize;
        PanelRight.rectTransform.sizeDelta = screenSize;

        // initialize animation params
        yFinalSpeed = (1-pauseTime*yInitialSpeed)/(1-yResumeTime);

        SetVisibility(StartVisible);
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayAnimation && timer < timerMax)
        {
            if (timer == 0)
            {
                SetVisibility(true);
                // idk why it makes me do this here but *shrug*
            }
            if (animDelay > 0)
            {
                animDelay -= Time.deltaTime;
            }
            else
            {
                timer += Time.deltaTime;
                float progress = Mathf.Min(1f, timer/timerMax);  
                // invert animation
                if (invertAnimation)
                {
                    progress = 1-progress;
                }
                // piecewise - open uniformly, then x continues uniformly but faster while y freezes 1/4 of the way in and resumes
                float progressX = progress*xSpeed;
                float progressY = progress*yInitialSpeed;
                if (progress >= pauseTime && progress < yResumeTime)
                {
                    progressY = pauseTime*yInitialSpeed;
                }
                else if (progress >= yResumeTime)
                {
                    progressY = pauseTime*yInitialSpeed+yFinalSpeed*(progress-yResumeTime);
                }
                
                UpdatePanels(progressX,progressY);
                
                // hide at the end of the animation
                if (timer >= timerMax && !invertAnimation)
                {
                    SetVisibility(false);
                }
            }
        }
    }

    /// <summary>
    /// Updates the position of the panels
    /// </summary>
    private void UpdatePanels(float progressX, float progressY)
    {
        PanelTop.transform.position = topStart - Vector3.up*height*progressY/2f;
        PanelBottom.transform.position = botStart + Vector3.up*height*progressY/2f;
        PanelLeft.transform.position = leftStart - Vector3.right*width*progressX/2f;
        PanelRight.transform.position = rightStart + Vector3.right*width*progressX/2f;
    }

    /// <summary>
    /// Start the bootup animation on command
    /// </summary>
    public void StartBootupSequence(float length)
    {
        invertAnimation = false;
        animDelay = bootupDelayFactor;
        UpdatePanels(0,0);
        StartAnimation(length);
    }

    /// <summary>
    /// Start the shutdown animation on command
    /// </summary>
    public void StartShutdownSequence(float length)
    {
        invertAnimation = true;
        animDelay = 0;
        UpdatePanels(1,1);
        StartAnimation(length);
    }

    /// <summary>
    /// Start an animation on command
    /// </summary>
    private void StartAnimation(float length)
    {
        PlayAnimation = true;
        timer = 0;
        timerMax = length;
        SetVisibility(true);
    }

    /// <summary>
    /// Toggle the state of the panels
    /// </summary>
    private void SetVisibility(bool show)
    {
        PanelTop.enabled = show;
        PanelBottom.enabled = show;
        PanelLeft.enabled = show;
        PanelRight.enabled = show;
    }
}
