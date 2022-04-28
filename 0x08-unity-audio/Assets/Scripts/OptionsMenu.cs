using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup musicMixer, sfxMixer;
    [SerializeField] private Slider musSlid, sfxSlid;
    public Toggle invertToggle;
    public PlayerController player;
    public GameObject pauseCanvas;
    public bool invertConfigY;
    public float sfxVol, musVol;

    void Start()
    {
        LoadConfig();
    }

    public void LevelSelect(int level)
    {
        StartCoroutine(LoadScene(level));
    }

    public void LoadConfig()
    {   
        if (File.Exists(Application.persistentDataPath + "/config.run"))
        {
            ConfigData cd = SaveSystem.LoadConfig();
            invertToggle.isOn = cd.invertY;
            Debug.Log(cd.musVol + " " + cd.sfxVol);
            musicMixer.audioMixer.SetFloat("vol", cd.musVol);
            sfxMixer.audioMixer.SetFloat("volsfx", cd.sfxVol);
            musSlid.value = sfxVol = cd.musVol;
            sfxSlid.value = musVol = cd.sfxVol;
        }
    }

    public void Apply()
    {
        if (player != null)
        {
            player.invertY = invertToggle.isOn;
        }

        invertConfigY = invertToggle.isOn;

        SaveSystem.SaveConfig(this);
    }
    
    public void SetMusicVolume (float value)
    {
        musicMixer.audioMixer.SetFloat("vol", value);
        musVol = value;
    }

    public void SetSFXVolume (float value)
    {
        sfxMixer.audioMixer.SetFloat("volsfx", value);
        sfxVol = value;
    }

    public void Back()
    {
        pauseCanvas.SetActive(true);
        gameObject.SetActive(false);
    }

    IEnumerator LoadScene(int sceneName)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncOperation.isDone)
        {
            yield return null;
        }
    }
}
