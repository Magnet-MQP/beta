using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns and repositions scrolling city buildings
/// </summary>

public class BuildingManager : MonoBehaviour
{
    public int BuildingCount = 100;
    public float RespawnDistance = 1000f;
    public GameObject BuildingTemplate;
    public GameObject PlayerRef; // These track the player to keep the buildings spawning around the player

    // Instantiate buildings
    void Start()
    {
        Vector3 thisPos = transform.position;
        float leftEdge = thisPos.x - transform.localScale.x/2;
        float rightEdge = thisPos.x + transform.localScale.x/2;
        for (int i = 0; i < BuildingCount; i++)
        {
            GameObject newBuilding = Instantiate(BuildingTemplate, 
                                                new Vector3(Random.Range(leftEdge, rightEdge), 0, 
                                                            Random.Range(thisPos.z, thisPos.z + RespawnDistance)),
                                                Quaternion.identity);
            CityScroll buildingScript = newBuilding.GetComponent<CityScroll>();
            buildingScript.LeftBound = leftEdge;
            buildingScript.RightBound = rightEdge;
            buildingScript.SetNewScale();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        CityScroll otherScript = other.GetComponent<CityScroll>();
        if (otherScript != null)
        {
            otherScript.Respawn(RespawnDistance);
        }
    }

    void Update()
    {
        transform.position = new Vector3(transform.position.x,
                                        transform.position.y,
                                        PlayerRef.transform.position.z - RespawnDistance/2);
    }
}
