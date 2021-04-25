using UnityEngine;

public class Menu_PlayAudio : MonoBehaviour
{
    public AudioSource UI_Source;
    public AudioClip Scroll_First;
    public AudioClip Scroll_Next;
    public AudioClip Select;
    // Start is called before the first frame update
    void Start()
    {
        UI_Source.ignoreListenerPause = true;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void play_scroll_first() {
        UI_Source.Stop();
        UI_Source.clip = Scroll_First;
        UI_Source.Play();
    }
    public void play_scroll_next() {
        UI_Source.Stop();
        UI_Source.clip = Scroll_Next;
        UI_Source.Play();
    }
    public void play_select() {
        UI_Source.Stop();
        UI_Source.clip = Select;
        UI_Source.Play();
    }
}
