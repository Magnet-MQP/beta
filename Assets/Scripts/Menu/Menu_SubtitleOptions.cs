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
    public void setSubtitleAlpha(float backAlpha = -1)
    {
        manager.SetSubtitleAlpha(backAlpha);
    }
}
