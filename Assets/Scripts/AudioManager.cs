using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip menuMusic;
    public AudioClip levelMusic;

    string sceneName;

    // Start is called before the first frame update
    void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene(); 
        sceneName = currentScene.name;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
