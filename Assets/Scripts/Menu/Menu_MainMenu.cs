﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu_MainMenu : MonoBehaviour
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
    public void ReturnToMain() {
        Debug.Log("hi");
        manager.mainMenu();
    }

}
