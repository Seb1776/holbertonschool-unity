using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class OptionsMenu : MonoBehaviour
{
    public Toggle invertToggle;
    public PlayerController player;
    public GameObject pauseCanvas;
    public bool invertConfigY;

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
