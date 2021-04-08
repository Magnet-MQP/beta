using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SupportedResolutions
{
    RES_1920X1080 = 0,
    RES_1600X900 = 1,
    RES_1024X576 = 2,
    RES_756X432 = 3,
    RES_512X288 = 4,
}

public class Menu_ChangeResolution : MonoBehaviour
{
    GameManager manager;
    // Start is called before the first frame update
    void Start()
    {
        manager = GameManager.getGameManager();
    }

    /// <summary>
    /// Applies the appropriate resolution for the supplied enumm value
    /// (unfortunately Unity restrictions force us to use an int instead,
    /// so use the enum above for reference on the actual values)
    /// </summary>
    public void ChangeResolution(int res)
    {
        int width = Screen.width;
        int height = Screen.height;

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

        manager.ChangeResolution(width, height);
    }
}
