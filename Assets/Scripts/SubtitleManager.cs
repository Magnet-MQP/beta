using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the display of subtitles.
/// Extra messages are stored in a list sorted by priority. Only the highest-
/// priority message actually gets displayed.
/// Message timers all decay continuously, and they are removed from the
/// list when they run out (whether they're currently displayed or not)
/// If a new, higher-priority message appears, it will display in place of 
/// whatever message was previously shown.
/// </summary>

/// TODO:
/// - If messages have a non-null position set, then display the sound 
///   direction with a visual indicator

public class SubtitleManager : MonoBehaviour
{
    // Static reference
    public static SubtitleManager Instance;
    // References
    [Tooltip("The text object used to display subtitles")]
    public TextMeshProUGUI Text;
    [Tooltip("The background panel behind the subtitle text")]
    public Image TextBack;
    // Display values
    [Tooltip("The font size for subtitles")]
    public int FontSize = 16;
    [Tooltip("The alpha of the black text background (0-1)")]
    public float BackAlpha = 1.0f;
    [Tooltip("The horizontal padding around the subtitle text")]
    public int BackPaddingX = 16;
    [Tooltip("The vertical padding around the subtitle text")]
    public int BackPaddingY = 16;
    // menu scaler
    private int subtitleScale = 1;
    private bool isSlow = false;
    private Vector3 originalPos = new Vector3(0,0,0);

    public GameObject menuParent;
    public GameObject canvas;

    // Subtitle messages
    [Tooltip("Stores the queue of upcoming subtitles to display")]
    private List<SubtitleData> subtitleQueue = new List<SubtitleData>();

    GameManager GM;

    // Initialize Instance to self
    void Awake()
    {
        Instance = this;
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
    void Start()
    {
        GM = GameManager.getGameManager();
        SetSubtitleSize();
        SetSubtitleAlpha();
        originalPos = transform.localPosition;
    }
    public static SubtitleManager getSubtitleManager() {
        return Instance;
    }

    // Update is called once per frame
    void Update()
    {
        // DEBUG - display test messages
        
        if (Keyboard.current.digit1Key.isPressed)
        {
            QueueSubtitle(new SubtitleData("This is a long subtitle test", 1000));
        }
        if (Keyboard.current.digit2Key.isPressed)
        {
            QueueSubtitle(new SubtitleData("Short test", 500, 1f));
        }
        if (Keyboard.current.digit3Key.isPressed)
        {
            QueueSubtitle(new SubtitleData("FEDOR: This is a ludicrously long dialogue test. Let's really force this to split some text yo!", 50, 6f));
        }
        

        // update timers
        bool changed = false;
        for (int i=subtitleQueue.Count-1; i >= 0; i--)
        {
            SubtitleData sd = subtitleQueue[i];
            sd.timer -= Time.deltaTime;
            // remove if timed out
            if (sd.timer <= 0)
            {
                subtitleQueue.Remove(sd);
                changed = true;
            }
        }

        // update appearance if necessary
        if (changed)
        {
            RefreshSubtitle();
        }
    }

    /// <summary>
    /// Queue a subtitle to display
    /// </summary>
    /// <param name="sd">New subtitle data</param>
    public void QueueSubtitle(SubtitleData sd)
    {
        SubtitleData scaledSub = new SubtitleData(sd.message,sd.priority,sd.timer*subtitleScale);
        sd = scaledSub;
        // Ignore if subtitle already queued
        if (subtitleQueue.Contains(sd))
        {
            return;
        }

        // Add subtitle
        subtitleQueue.Add(sd);

        // Re-order subtitles by priority
        subtitleQueue.Sort((sd1,sd2) => (-sd1.priority).CompareTo(-sd2.priority));

        // Update appearance
        RefreshSubtitle();
    }

    /// <summary>
    /// Update the subtitle displayed
    /// </summary>
    private void RefreshSubtitle()
    {
        // Hide if queue empty
        if (subtitleQueue.Count <= 0)
        {
            TextBack.gameObject.SetActive(false);
        }
        // Reveal and scale properly otherwise
        else
        {
            // display subtitle
            TextBack.gameObject.SetActive(true);

            // get message text
            string newText = subtitleQueue[0].message;
            
            // get text size
            Vector2 subtitleSize = Text.GetPreferredValues(newText) + new Vector2(BackPaddingX, BackPaddingY);
            
            // resize text object
            Text.gameObject.GetComponent<RectTransform>().sizeDelta = subtitleSize;
            
            // apply new text
            Text.text = newText;

        }
    }

    /// <summary>
    /// Change the font size of the subtitle
    /// Calling without an argument just applies the current size
    /// </summary>
    /// <param name="fontSize">[Optional] the new font size (defaults to current)</param>
    public void SetSubtitleSize(float fontSize = -1)
    {
        if (fontSize > 0)
        {
            FontSize = (int) fontSize;
        }
        Text.fontSize = FontSize;
        RefreshSubtitle();
    }

    /// <summary>
    /// Change the font size of the subtitle
    /// Calling without an argument just applies the current alpha
    /// </summary>
    /// <param name="backAlpha">[Optional] the new alpha (defaults to current)</param>
    public void SetSubtitleAlpha(float backAlpha = -1)
    {
        if (backAlpha > 0)
        {
            BackAlpha = backAlpha;
        }
        TextBack.color = new Vector4(0,0,0, BackAlpha);
    }

    /// <summary>
    /// Change the horizontal padding between the text and backdrop edge 
    /// (mirrored on each side)
    /// </summary>
    /// <param name="pad">New padding value</param>
    public void SetSubtitlePaddingX(int pad)
    {
        BackPaddingX = pad*2;
        RefreshSubtitle();
    }

    /// <summary>
    /// Change the vertical padding between the text and backdrop edge 
    /// (mirrored on each side)
    /// </summary>
    /// <param name="pad">New padding value</param>
    public void SetSubtitlePaddingY(int pad)
    {
        BackPaddingY = pad*2;
        RefreshSubtitle();
    }

    // only for menu subtitle move
    public void moveSubtitlesForMenu() 
    {
        transform.SetParent(menuParent.transform);
        transform.localPosition = new Vector3(0, 0, 0); 
    }

    // only for removing subtitle move
    public void removeSettingsSubtitle() {
        subtitleQueue.RemoveAt(0);
        transform.SetParent(canvas.transform);
        transform.localPosition = originalPos;
        transform.SetSiblingIndex(6);

        RefreshSubtitle();
    }

    // doubles the length of all subtitles
    public void slowSubtitles() {
        isSlow = !isSlow;
        if (isSlow)
        {
            foreach (SubtitleData subtitle in subtitleQueue) 
            {
                subtitle.timer = subtitle.timer * 2;
            }
            subtitleScale = 2;
        }
        else 
        {
            foreach (SubtitleData subtitle in subtitleQueue) 
            {
                subtitle.timer = subtitle.timer / 2;
            }
            subtitleScale = 1;
        }
    }

}
