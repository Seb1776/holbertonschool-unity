using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerTrigger : MonoBehaviour
{
    void OnTriggerExit(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Timer>().canCount = true;
            other.GetComponent<Timer>().startPlaying.SetActive(false);
        }
    }
}
