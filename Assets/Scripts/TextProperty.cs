using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to an object to assign it readable text
/// </summary>

public class TextProperty : MonoBehaviour
{
    [Tooltip("The text to display when examined")]
    public string Text;
    [Tooltip("The maximum distance it can be read from. Negative if infinite")]
    public float ReadingDistance = -1;
}
