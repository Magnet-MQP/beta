using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;

public class LookController : MonoBehaviour {
    private PlayerInput m_PlayerInput;
    GameManager manager;

    void Start()
    {
        manager = GameObject.FindGameObjectsWithTag("GameManager")[0].GetComponent<GameManager>();
        m_PlayerInput = manager.getPlayerInput();
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