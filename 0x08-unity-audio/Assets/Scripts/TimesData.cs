using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TimesData
{
    public float newTime;

    public TimesData (Timer timer)
    {
        newTime = timer.timerTime;
    }
}
