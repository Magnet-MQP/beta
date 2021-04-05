using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Menu_AudioSettings : MonoBehaviour
{
    public AudioMixer masterMix;
    public AudioMixer envMix;
    public AudioMixer sfxMix;
    public AudioMixer uiMix;
    // Start is called before the first frame update
    void Start()
    {

    }

    /// <summary>
    /// Change the font size of the subtitle
    /// Calling without an argument just applies the current size
    /// </summary>
    /// <param name="masterVolume">[Optional] the new font size (defaults to current)</param>
    public void SetMasterVolume(float masterVolume = -1)
    {
        masterMix.SetFloat("masterVolume", masterVolume);
    }

    /// <summary>
    /// Change the font size of the subtitle
    /// Calling without an argument just applies the current size
    /// </summary>
    /// <param name="environmentVolume">[Optional] the new font size (defaults to current)</param>
    public void SetEnvironmentVolume(float environmentVolume = -1)
    {
        envMix.SetFloat("environmentVolume", environmentVolume);
    }

    /// <summary>
    /// Change the font size of the subtitle
    /// Calling without an argument just applies the current size
    /// </summary>
    /// <param name="sfxVolume">[Optional] the new font size (defaults to current)</param>
    public void SetSfxVolume(float sfxVolume = -1)
    {
        sfxMix.SetFloat("sfxVolume", sfxVolume);
    }

    /// <summary>
    /// Change the font size of the subtitle
    /// Calling without an argument just applies the current size
    /// </summary>
    /// <param name="uiVolume">[Optional] the new font size (defaults to current)</param>
    public void SetUiVolume(float uiVolume = -1)
    {
        uiMix.SetFloat("uiVolume", uiVolume);
    }
}
