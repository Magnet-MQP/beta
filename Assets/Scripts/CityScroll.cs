using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls city building objects to simulate them moving
/// </summary>

public class CityScroll : MonoBehaviour
{
    [Tooltip("The model attached to this representing the building")]
    public Renderer BuildingModel;
    [Tooltip("The index numbers of the window materials")]
    public int[] WindowMaterialIndexes;
    public float MoveSpeed = 4f;
    public Vector3 MinSize = new Vector3(150, 200, 150);
    public Vector3 MaxSize = new Vector3(200, 500, 200);
    public float LeftBound;
    public float RightBound;
    private Rigidbody rb;
    private float respawnDistance = 0f;
    private float respawnWait = 0f;
    private float respawnWaitMax = 15f; // This determines how long the building takes to fade in and out
    private bool respawning = false;
    private bool spawned = false;
    private Color baseEmissionColor;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // initialize all window materials for emission adjustment
        for (int i = 0; i < WindowMaterialIndexes.Length; i++)
        {
            BuildingModel.materials[WindowMaterialIndexes[i]].EnableKeyword("_EMISSION");
        }
        // store the initial emissive color
        baseEmissionColor = BuildingModel.materials[WindowMaterialIndexes[0]].GetColor("_EmissiveColor");
    }

    // Resize the building and reposition accordingly
    public void SetNewScale()
    {
        transform.localScale = new Vector3(Random.Range(MinSize.x, MaxSize.x),
                                        Random.Range(MinSize.y, MaxSize.y),
                                        Random.Range(MinSize.z, MaxSize.z));
        float width = transform.localScale.x;
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, LeftBound + width, RightBound - width),
                                        transform.localScale.y/2,
                                        transform.position.z);
    }

    public void Respawn(float distance)
    {
        respawning = true;
        respawnWait = 0f;
        respawnDistance = distance;
    }

    /// <summary>
    /// Called internally to actually carry out respawn
    /// </summary>
    private void PerformRespawn()
    {
        // update position
        transform.position += Vector3.forward * respawnDistance;
        SetNewScale();
        // momentarily hide building
        BuildingModel.enabled = false;
        // set state to respaning
        respawning = false;
        spawned = false;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position - Vector3.forward * MoveSpeed);

        // increase wait if respawning
        if (respawning)
        {
            respawnWait += Time.deltaTime*2f;
            // perform respawn if fully faded in
            if (respawnWait >= respawnWaitMax)
            {
                respawnWait = respawnWaitMax;
                PerformRespawn();
            }
        }
        // decrease wait if spawning back in
        else if (respawnWait > 0)
        {
            respawnWait = Mathf.Max(0, respawnWait-Time.deltaTime);
        }
        // adjust window emission based on wait (fade in and out at ends)
        if (respawnWait > 0)
        {
            Color newEmissive = baseEmissionColor*(1-respawnWait/respawnWaitMax);
            for (int i = 0; i < WindowMaterialIndexes.Length; i++)
            {
                BuildingModel.materials[WindowMaterialIndexes[i]].SetColor("_EmissiveColor", newEmissive);
            }
        }

        // spawn in
        if (!spawned)
        {
            BuildingModel.enabled = true;
            spawned = true;
        }
    }

    // prevent overlap with other buildings
    void OnTriggerEnter(Collider other)
    {
        if (!spawned && other.GetComponent<CityScroll>() != null)
        {
            transform.position += Vector3.forward * other.transform.localScale.z/2f;
        }
    }
 

}
