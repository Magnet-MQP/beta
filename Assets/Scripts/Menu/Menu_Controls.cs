using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu_Controls : MonoBehaviour
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

    public void Controls()
    {
        manager.showControlsMenu();
    }

    public void changeLookSpeedX(float value)
    {
        manager.lookSpeedX = value;
    }

    public void changeLookSpeedY(float value) 
    {
        manager.lookSpeedY = value;
    }

    public void invertX()
    {
        manager.lookSpeedX = manager.lookSpeedX * (-1.0f);  
    }

    public void invertY()
    {
        manager.lookSpeedY = manager.lookSpeedY * (-1.0f);  
    }

    public void toggleHold()
    {
        manager.glovesIsHold = !manager.glovesIsHold;
    }
}
