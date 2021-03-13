using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu_Subtitles : MonoBehaviour
{
    GameManager manager;
    // Start is called before the first frame update
    void Start()
    {
        manager = GameManager.getGameManager();
    }

    public void Subtitles()
    {
        manager.showSubtitleMenu();

    }
}
