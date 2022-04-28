using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject optionsMenu;
    public GameObject mainCanvas;

    public void LevelSelect(int level)
    {
        StartCoroutine(LoadScene(level));
    }

    public void Options()
    {
        optionsMenu.SetActive(true);
        mainCanvas.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
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
