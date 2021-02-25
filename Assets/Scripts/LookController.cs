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
    }

    public void changeLookX(float value)
    {
       // manager.lookSpeedX = value;
    }

    public void changeLookY(float value) 
    {
        //manager.lookSpeedY = value;
    }
}