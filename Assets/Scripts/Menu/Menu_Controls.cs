using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu_Controls : MonoBehaviour
{
    GameManager manager;
    bool invert_x = false;
    bool invert_y = false;
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

    public void changeLookSpeedMouseX(float value)
    {
        if(invert_x)
        {
          value = value * (-1f);
        }
        manager.lookSpeedMouseX = value;
    }

    public void changeLookSpeedMouseY(float value) 
    {
        if(invert_y)
        {
          value = value * (-1f);
        }
        manager.lookSpeedMouseY = value;
    }

    public void changeLookSpeedControllerX(float value)
    {
        if(invert_x)
        {
          value = value * (-1f);
        }
        manager.lookSpeedControllerX = value;
    }

    public void changeLookSpeedControllerY(float value) 
    {
        if(invert_y)
        {
          value = value * (-1f);
        }
        manager.lookSpeedControllerY = value;
    }

    public void invertX()
    {
        invert_x = !invert_x;
        manager.lookSpeedControllerX = manager.lookSpeedControllerX * (-1.0f);  
        manager.lookSpeedMouseX = manager.lookSpeedMouseX * (-1.0f);  
    }

    public void invertY()
    {
        invert_y = !invert_y;
        manager.lookSpeedControllerY = manager.lookSpeedControllerY * (-1.0f);  
        manager.lookSpeedMouseY = manager.lookSpeedMouseY * (-1.0f);  
    }

    public void toggleHold()
    {
        manager.glovesIsHold = !manager.glovesIsHold;
    }
}
