using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns and repositions scrolling city buildings
/// </summary>

public class BuildingManager : MonoBehaviour
{
    [Tooltip("The number of buildings to instantiate")]
    public int BuildingCount = 100;
    [Tooltip("The distance away at which to spawn buildings")]
    public float RespawnDistance = 1000f;
    [Tooltip("The set of buildings to use")]
    public GameObject[] BuildingTemplates;
    [Tooltip("The scene's player object")]
    private GameObject playerRef; // These track the player to keep the buildings spawning around the player

    // Instantiate buildings
    void Start()
    {
        playerRef = GameManager.Instance.GetPlayer();
        Vector3 thisPos = transform.position;
        float leftEdge = thisPos.x - transform.localScale.x/2;
        float rightEdge = thisPos.x + transform.localScale.x/2;
        for (int i = 0; i < BuildingCount; i++)
        {
            GameObject newBuilding = Instantiate(BuildingTemplates[Random.Range(0,BuildingTemplates.Length)], 
                                                new Vector3(Random.Range(leftEdge, rightEdge), 0, 
                                                            Random.Range(thisPos.z, thisPos.z + RespawnDistance)),
                                                Quaternion.identity);
            // set dimensions
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
        if (playerRef != null)
        {
            transform.position = new Vector3(transform.position.x,
                                            transform.position.y,
                                            playerRef.transform.position.z - RespawnDistance*0.7f);
        }
    }
}
