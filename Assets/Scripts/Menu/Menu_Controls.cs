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

    public void changeLookSpeedMouseX(float value)
    {
        manager.lookSpeedMouseX = value;
    }

    public void changeLookSpeedMouseY(float value) 
    {
        manager.lookSpeedMouseY = value;
    }

    public void changeLookSpeedControllerX(float value)
    {
        manager.lookSpeedControllerX = value;
    }

    public void changeLookSpeedControllerY(float value) 
    {
        manager.lookSpeedControllerY = value;
    }

    public void invertX()
    {
        manager.lookSpeedControllerX = manager.lookSpeedControllerX * (-1.0f);  
        manager.lookSpeedMouseX = manager.lookSpeedMouseX * (-1.0f);  
    }

    public void invertY()
    {
        manager.lookSpeedControllerY = manager.lookSpeedControllerY * (-1.0f);  
        manager.lookSpeedMouseY = manager.lookSpeedMouseY * (-1.0f);  
    }

    public void toggleHold()
    {
        manager.glovesIsHold = !manager.glovesIsHold;
    }
}
