using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCutscene", menuName = "ScriptableObjects/Cutscene")]
public class CutsceneData : ScriptableObject
{
    [Tooltip("The full time length of the cutscene")]
    public float FullDuration;
    [Tooltip("The length of time in the cutscene where the player's input is disabled")]
    public float LockDuration;
    
    [Tooltip("Whether to show the bootup \"eyes opening\" animation")]
    public bool ShowBootup = false;
    [Tooltip("Whether to return to the menu at the end of the cutscene")]
    public bool ExitAtEnd = false;
    
    [Tooltip("The set of dialogue lines to play in the cutscene")]
    public SubtitleData[] Dialogue;
}
