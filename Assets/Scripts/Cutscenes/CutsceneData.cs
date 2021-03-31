using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCutscene", menuName = "ScriptableObjects/Cutscene")]
public class CutsceneData : ScriptableObject
{
    [Tooltip("The full time length of the cutscene")]
    [Header("Duration")]
    public float FullDuration;
    [Tooltip("The length of time in the cutscene where the player's input is disabled")]
    public float LockDuration;
    
    [Tooltip("Whether to show the bootup \"eyes opening\" animation")]
    [Header("Settings")]
    public bool ShowBootup = false;
    [Tooltip("Whether to show the shutdown \"eyes closing\" animation")]
    public bool ShowShutdown = false;
    [Tooltip("The length of time to play the shutdown animation")]
    public float ShutdownDuration = 1f;
    [Tooltip("Whether to return to the menu at the end of the cutscene")]
    public bool ExitAtEnd = false;
    [Tooltip("Whether to advance to the next scene at the end of the cutscene")]
    public bool AdvanceAtEnd = false;
    [Tooltip("Whether to reload the scene at the end of the cutscene")]
    public bool ReloadAtEnd = false;
    
    [Tooltip("The set of dialogue lines to play in the cutscene")]
    [Header("Dialogue")]
    public SubtitleData[] Dialogue;
}
