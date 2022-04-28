using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseCanvas;
    public GameObject optionsCanvas;
    public AudioSource musicSystem;
    public PlayerController player;

    bool paused;
    bool canPause = true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && canPause)
        {
            if (!paused)
                Pause();
            
            else
                Resume();
        }
    }

    public void Pause()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        player.GetComponent<PlayerController>().canMove = player.GetComponent<PlayerController>().canRotate = false;
        Time.timeScale = 0f;
        musicSystem.pitch = 0f;
        pauseCanvas.SetActive(true);
        paused = true;
    }

    public void Resume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        player.GetComponent<PlayerController>().canMove = player.GetComponent<PlayerController>().canRotate = true;
        Time.timeScale = 1f;
        musicSystem.pitch = 1f;
        pauseCanvas.SetActive(false);
        paused = false;
    }

    public void Restart()
    {
        player.RetryGame();
        Resume();
    }

    public void MainMenu()
    {
        LevelSelect(3);
    }

    public void Options()
    {
        //LevelSelect(4);
        pauseCanvas.SetActive(false);
        optionsCanvas.SetActive(true);
    }

    public void LevelSelect(int level)
    {
        StartCoroutine(LoadScene(level));
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
