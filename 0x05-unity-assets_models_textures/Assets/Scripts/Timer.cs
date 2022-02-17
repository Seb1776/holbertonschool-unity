using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Text timer, winTimer, bestTimeText;
    public GameObject winPanel;
    public GameObject startPlaying;
    public float timerTime;
    public float timeFactor;
    public float bestTime;
    public bool canCount;
    public bool winGame;

    void Awake()
    {
        LoadBestTime();
    }

    void Update()
    {   
        if (canCount && !winGame)
        {
            timerTime += Time.deltaTime;
            timer.text = TimeSpan.FromSeconds(timerTime).Minutes.ToString() + ":" + TimeSpan.FromSeconds(timerTime).Seconds.ToString("D2") + ":" +
                        TimeSpan.FromSeconds(timerTime).Milliseconds.ToString();
        }
    }

    public void SaveBestTime()
    {
        SaveSystem.SaveTime(this);
    }

    public void LoadBestTime()
    {
        TimesData td = SaveSystem.LoadTimes();

        if (td != null)
        {
            Debug.Log(td.newTime);

            bestTime = td.newTime;
        }
    }

    public void TriggerWinGame()
    {
        winGame = true;
        winTimer.color = Color.green;

        if (timerTime < bestTime || bestTime <= 0f)
        {            
            bestTimeText.text = TimeSpan.FromSeconds(timerTime).Minutes.ToString() + ":" + TimeSpan.FromSeconds(timerTime).Seconds.ToString("D2") + ":" +
            TimeSpan.FromSeconds(timerTime).Milliseconds.ToString();
            bestTime = timerTime;
        }

        else
        {
            bestTimeText.text = TimeSpan.FromSeconds(bestTime).Minutes.ToString() + ":" + TimeSpan.FromSeconds(bestTime).Seconds.ToString("D2") + ":" +
            TimeSpan.FromSeconds(bestTime).Milliseconds.ToString();
        }

        winTimer.text = timer.text;
        winPanel.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GetComponent<PlayerController>().canMove = GetComponent<PlayerController>().canRotate = false;
    }
}
