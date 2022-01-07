using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    float rotVar;

    void Update()
    {
        rotVar += Time.deltaTime * 45;

        if (rotVar > 360f)
            rotVar = 0f;

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, rotVar, transform.eulerAngles.z);
    }
}
