using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform lookAt;
    public Transform camTransform;
    public Vector2 clampCamera;
    public float sensitivity = 3f;

    Camera cam;
    float distance = 6.25f;
    float currentX = 0f;
    float currentY = 0f;

    
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    
        camTransform = transform;
        cam = Camera.main;
    }

    void Update()
    {   
        if (lookAt.GetComponent<PlayerController>().canRotate)
        {
            if (!lookAt.GetComponent<PlayerController>().useController || Input.GetJoystickNames().Length < 0)
            {
                currentX += Input.GetAxisRaw("Mouse X");
                currentY -= Input.GetAxisRaw("Mouse Y");
            }

            else if (lookAt.GetComponent<PlayerController>().useController && Input.GetJoystickNames().Length > 0)
            {
                currentX += Input.GetAxisRaw("Joystick X");
                currentY -= Input.GetAxisRaw("Joystick Y");
            }

            currentY = Mathf.Clamp(currentY, clampCamera.x, clampCamera.y);
        }
    }

    void LateUpdate()
    {
        if (lookAt.GetComponent<PlayerController>().canRotate)
        {
            Vector3 direction = new Vector3 (0, 0, -distance);
            Quaternion rotation = Quaternion.Euler(currentY * sensitivity, currentX * sensitivity, 0);

            camTransform.position = lookAt.position + rotation * direction;
            camTransform.LookAt(lookAt.position);
        }
    }
}
