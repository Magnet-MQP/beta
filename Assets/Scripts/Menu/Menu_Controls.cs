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
}
