using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentLoader_L1 : MonoBehaviour
{

    [Tooltip("The set of of segments in the group")]
    public GameObject[] Segments;
    // Start is called before the first frame update
    void Start()
    {
        ActivateSegment(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            ActivateSegment(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            //ActivateSegment(false);
        }
    }

    private void ActivateSegment(bool state)
    {
        foreach (GameObject segment in Segments)
        {
            segment.SetActive(state);
        }
    }
}
