using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Put this on the part of an interactable object that gets selected and use it to reference the part of the object that will need to get outlined
/// </summary>

public class Outliner : MonoBehaviour
{
    public GameObject OutlineTarget;
    [Tooltip("The name of the outline layer to apply")]
    public string OutlineLayerType;
}
