using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    public string sceneToLoad;

    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
            ChangeScene(sceneToLoad);
    }

    public void ChangeScene(string sceneName)
    {
        StartCoroutine(LoadScene(sceneName));
    }

    IEnumerator LoadScene(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        while (op.isDone)
        {
            yield return null;
        }
    }
}
