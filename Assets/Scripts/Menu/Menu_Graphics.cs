using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu_Graphics : MonoBehaviour
{
    GameManager manager;
    // Start is called before the first frame update
    void Start()
    {
        manager = GameManager.getGameManager();
    }

    public void Graphics()
    {
        manager.showGraphicsMenu();

    }
}
