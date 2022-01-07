using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;

    void Start()
    {
        if (player == null)
            Debug.Log("Player is not set.");
    }

    void Update()
    {
        if (player != null)
            transform.position = new Vector3(player.transform.position.x, 26f, player.transform.position.z);
    }
}
