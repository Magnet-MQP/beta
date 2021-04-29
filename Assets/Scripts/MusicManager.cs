using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    private AudioSource source;
    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        if(!source.isPlaying){
            Debug.Log("Play");
            source.Play(0);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static MusicManager getMusicManager() {
        return Instance;
    }

    void Awake() {
        if(Instance == null) {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if(Instance != this) {
            Destroy(gameObject);
        }
    }
}
