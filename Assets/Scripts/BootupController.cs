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
    
    private float timer = 0;
    private float timerMax = 15f;
    private bool invertAnimation = false; // whether to play the animation in reverse
   

    void Awake()
    {
        width = Screen.width;
        height = Screen.height;

        topStart = PanelTop.transform.position;
        botStart = PanelBottom.transform.position;
        leftStart = PanelLeft.transform.position;
        rightStart = PanelRight.transform.position;

        SetVisibility(StartVisible);
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayAnimation && timer < timerMax)
        {
            float timerFactor = 15f/timerMax; // 15 is a magic number from how this was originally coded
            timer += Time.deltaTime;
            float progressX = Mathf.Min(1f, timer/timerMax);  
            // invert animation
            if (invertAnimation)
            {
                progressX = 1-progressX;
            }
            // piecewise - x continues uniformly, while y freezes 1/4 of the way in and resumes
            float progressY = progressX;
            if (progressX >= 0.25f && progressX < 0.75f)
            {
                progressY = 0.25f;
            }
            else if (progressX >= 0.75f)
            {
                progressY = 0.25f+3f*(progressX-0.75f);
            }
            
            PanelTop.transform.position = topStart - Vector3.up*height*progressY;
            PanelBottom.transform.position = botStart + Vector3.up*height*progressY;
            PanelLeft.transform.position = leftStart - Vector3.right*width*progressX;
            PanelRight.transform.position = rightStart + Vector3.right*width*progressX;

            // hide at the end of the animation
            if (timer >= timerMax)
            {
                SetVisibility(false);
            }
        }
    }

    /// <summary>
    /// Start the bootup animation on command
    /// </summary>
    public void StartBootupSequence(float length)
    {
        invertAnimation = false;
        StartAnimation(length);
    }

    /// <summary>
    /// Start the shutdown animation on command
    /// </summary>
    public void StartShutdownSequence(float length)
    {
        invertAnimation = true;
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
    public void SetVisibility(bool show)
    {
        PanelTop.enabled = show;
        PanelBottom.enabled = show;
        PanelLeft.enabled = show;
        PanelRight.enabled = show;
    }
}
