using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

public class GameManager : MonoBehaviour
{
    public Color PS4BarLightColor;
    public float currentAnimSpeed = 0f;

    Animator animator;
    DualShockGamepad ps4Gamepad;

    void Start()
    {
        animator = GetComponent<Animator>();
        ps4Gamepad = (DualShockGamepad)Gamepad.all[0];
    }

    void Update()
    {
        animator.speed = currentAnimSpeed;
        ps4Gamepad.SetLightBarColor(PS4BarLightColor);

        if (ps4Gamepad.crossButton.IsPressed())
        {
            if (currentAnimSpeed < 1f)
                currentAnimSpeed += Time.deltaTime;
        }

        else
        {
            if (currentAnimSpeed > 0f)
            {
                currentAnimSpeed -= Time.deltaTime;

                if (currentAnimSpeed < 0f)
                    currentAnimSpeed = 0f;
            }
        }
    }
}
