using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public AudioSource[] music;
    public int currentlyPlaying = -1;
    public float fadeDir = 5f;
    public float maxVolume = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Advance the music to the next track, for level 1's dynamic music
    /// </summary>
    public void PlayNextTrack() {
        if(currentlyPlaying != -1) {
            StartCoroutine(FadeOut(music[currentlyPlaying], fadeDir));
        }
        currentlyPlaying += 1;
        StartCoroutine(FadeIn(music[currentlyPlaying], fadeDir, maxVolume));
    }

    public static IEnumerator FadeIn(AudioSource audioSource, float fadeTime, float maxVolume) {
        
        while (audioSource.volume < maxVolume) {
            audioSource.volume += Time.deltaTime / fadeTime;

            yield return null;
        }
        audioSource.volume = maxVolume;
    }

    public static IEnumerator FadeOut(AudioSource audioSource, float fadeTime) {
        float startVolume = audioSource.volume;
        
        while (audioSource.volume > 0) {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;

            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = startVolume;
    }
    public static IEnumerator FadeOutButKeepPlaying(AudioSource audioSource, float fadeTime) {
        //Debug.Log("Fade start.");
        float startVolume = audioSource.volume;
        
        while (audioSource.volume > 0) {
            //Debug.Log("Fading.");
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;

            yield return null;
        }
        //Debug.Log("Faded.");
        audioSource.volume = startVolume;
    }
}
