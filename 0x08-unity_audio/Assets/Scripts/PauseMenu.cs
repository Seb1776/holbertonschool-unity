using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseCanvas;
    public GameObject optionsCanvas;
    public AudioSource musicSystem;
    public PlayerController player;
    [SerializeField] private AudioMixerSnapshot pausedSnapshot, unPausedSnapshot;

    bool paused;
    public bool canPause = true;

    void Start()
    {
        unPausedSnapshot.TransitionTo(.01f);
    }

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
        pauseCanvas.SetActive(true);
        paused = true;

        pausedSnapshot.TransitionTo(.01f);
    }

    public void Resume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        player.GetComponent<PlayerController>().canMove = player.GetComponent<PlayerController>().canRotate = true;
        Time.timeScale = 1f;
        pauseCanvas.SetActive(false);
        paused = false;

        unPausedSnapshot.TransitionTo(.01f);
    }

    public void Restart()
    {
        unPausedSnapshot.TransitionTo(.01f);
        player.RetryGame();
        Resume();
    }

    public void MainMenu()
    {
        unPausedSnapshot.TransitionTo(.01f);
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
        unPausedSnapshot.TransitionTo(.01f);

        while (!asyncOperation.isDone)
        {
            yield return null;
        }
    }
}
