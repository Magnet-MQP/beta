using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSupplyController : MonoBehaviour
{
    [Tooltip("The set of sockets attached to the Power Supply")]
    public PowerSocketController[] Sockets;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // todo: track socket progress
        //if ()
    }
}
