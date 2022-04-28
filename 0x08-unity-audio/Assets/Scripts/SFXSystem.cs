using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXSystem : MonoBehaviour
{
    [SerializeField] private SoundEffect[] soundEffects;

    private AudioSource source;

    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlaySound(string _clipID)
    {
        foreach (SoundEffect se in soundEffects)
        {   
            if (se.clipID == _clipID)
            {
                source.PlayOneShot(se.clip);
                break;
            }
        }
    }
}

[System.Serializable]
public class SoundEffect
{
    public AudioClip clip;
    public string clipID;
}
