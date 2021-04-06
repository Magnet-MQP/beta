using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackButton : MonoBehaviour
{
    GameManager manager;
    // Start is called before the first frame update
    void Start()
    {
        manager = GameManager.getGameManager();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BackPause()
    {
      manager.showPauseMenu();
    }

    public void BackSettings()
    {
      manager.showSettingsMenu();
    }

    public void ShowMainMenu() {
      manager.showMainMenu();
    }

}
