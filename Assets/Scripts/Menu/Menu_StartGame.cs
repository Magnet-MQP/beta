﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu_StartGame : MonoBehaviour
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
    public void StartGame() {
        manager.nextScene();
    }

}
