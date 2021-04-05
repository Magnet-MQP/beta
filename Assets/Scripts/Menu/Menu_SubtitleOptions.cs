using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu_SubtitleOptions : MonoBehaviour
{
    SubtitleManager manager;
    // Start is called before the first frame update
    void Start()
    {
        manager = SubtitleManager.getSubtitleManager();
    }

    public void toggleSlowSubtitles()
    {
        manager.slowSubtitles();
    }
    /// <summary>
    /// Change the font size of the subtitle
    /// Calling without an argument just applies the current alpha
    /// </summary>
    /// <param name="backAlpha">[Optional] the new alpha (defaults to current)</param>
    public void setSubtitleAlpha(float backAlpha = -1)
    {
        manager.SetSubtitleAlpha(backAlpha);
    }

    /// <summary>
    /// Change the font size of the subtitle
    /// Calling without an argument just applies the current alpha
    /// </summary>
    /// <param name="fontSize">[Optional] the new alpha (defaults to current)</param>
    public void setSubtitleSize(float backAlpha = -1)
    {
        manager.SetSubtitleSize(backAlpha);
    }
}
