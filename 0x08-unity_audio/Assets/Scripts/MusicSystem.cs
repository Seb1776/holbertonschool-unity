using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSystem : MonoBehaviour
{
    public AudioClip startMusic;
    public AudioClip music;

    bool stoppingMusic;
    bool playMusic;
    AudioSource source;

    void Start()
    {
        source = GetComponent<AudioSource>();

        PlayMusic();
    }

    void Update()
    {   
        if (startMusic != null)
        {
            if (source.clip == startMusic && !source.isPlaying && !playMusic)
            {
                source.clip = music;
                source.loop = true;
                source.Play();
                playMusic = true;
            }
        }

        CheckMusicStop();
    }

    public void StopMusic()
    {
        stoppingMusic = true;
    }

    public void ReplayMusic()
    {
        playMusic = false;
        PlayMusic();
    }

    public void PlayMusic()
    {
        if (source.volume <= 0f)
            source.volume = 1f;
        
        if (stoppingMusic)
            stoppingMusic = false;

        if (startMusic != null)
        {
            source.clip = startMusic;
            source.loop = false;
            source.Play();
        }

        else
        {
            source.clip = music;
            source.loop = true;
            source.Play();
            playMusic = true;
        }
    }

    void CheckMusicStop()
    {
        if (stoppingMusic && source.volume > 0f)
        {
            source.volume -= Time.deltaTime;
        }

        else if (stoppingMusic && source.volume <= 0f)
        {
            source.Stop();
            playMusic = false;
        }
    }
}
