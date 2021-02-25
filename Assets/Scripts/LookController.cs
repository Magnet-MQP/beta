using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LookController : MonoBehaviour {
    private Slider lookX;
    private Slider lookY;
    GameManager manager;

    void Awake()
    {
        manager = GameObject.FindGameObjectsWithTag("GameManager")[0].GetComponent<GameManager>();
        Debug.Log(manager.lookSpeedX);
        Debug.Log(manager.lookSpeedY);
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
}