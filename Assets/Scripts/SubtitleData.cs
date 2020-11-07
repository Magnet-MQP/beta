using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data class used to represent a subtitle message and its metadata
/// </summary>

[System.Serializable]
public class SubtitleData
{
    [Tooltip("The message to display")]
    public string message;
    [Tooltip("The priority rank of the message. Higher is more important")]
    public int priority;
    [Tooltip("The display duration of the message, in seconds")]
    public float timer;
    [Tooltip("The position of the subtitle source, if applicable. null otherwise")]
    public Vector3? position;

    // Constructor
    public SubtitleData(string msg, int pr, float dur = -1f, Vector3? pos = null)
    {
        // set priority and position directly
        priority = pr;
        position = pos; // if position is null, subtitle is non-directional
        
        // automatically insert line breaks where needed into message
        string splitMessage = msg;
        int charPos = 0;
        int lastLine = 0;
        int lastBreak = 0;
        while (charPos < splitMessage.Length)
        {
            // update last break
            char curChar = splitMessage[charPos];
            if (curChar == ' ' || curChar == '-' || curChar == '\n')
            {
                lastBreak = charPos;
                if (curChar == '\n')
                {
                    lastLine = charPos+1;
                }
            }
            // detect if over line length (38 characters)
            if (charPos-lastLine > 38)
            {
                // insert a line break
                splitMessage = splitMessage.Insert(lastBreak, "\n");
                lastLine = lastBreak+1;
                charPos++;
            }
            // advance to next character
            charPos++;
        }
        message = splitMessage;

        // set timer or auto-calculate from message length
        if (dur < 0)
        {
            timer = msg.Length * 0.15f;
        }
        else
        {
            timer = dur;
        }
    }

    // Override Equals to just check for message, priority, and position
    public override bool Equals(System.Object other)
    {
        if (other == null)
        {
            return false;
        }

        SubtitleData sd = other as SubtitleData;
        if ((System.Object)sd == null)
        {
            return false;
        }

        return (message == sd.message) && (priority == sd.priority) && (position == sd.position);
    }

    // Use default HashCode generation system
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
