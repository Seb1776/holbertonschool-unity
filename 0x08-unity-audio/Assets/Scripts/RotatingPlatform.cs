using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingPlatform : MonoBehaviour
{
    public float rotationSpeed;
    public RotateProperties[] rotateTowards;

    void Start()
    {
        foreach (RotateProperties rp in rotateTowards)
            rp.tmpTimeToChangeDirection = rp.timeToChangeDirection;
    }

    void Update()
    {   
        for (int i = 0; i < rotateTowards.Length; i++)
        {
            if (rotateTowards[i].rotate)
            {
                if (rotateTowards[i].currentTimeToChangeDirection >= rotateTowards[i].tmpTimeToChangeDirection)
                {
                    rotateTowards[i].startingDirection = !rotateTowards[i].startingDirection;
                    
                    if (!rotateTowards[i].gottem)
                    {
                        rotateTowards[i].tmpTimeToChangeDirection = rotateTowards[i].timeToChangeDirection * 2f;
                        rotateTowards[i].gottem = true;
                    }

                    rotateTowards[i].currentTimeToChangeDirection = 0f;
                }

                else
                {
                    switch (i)
                    {
                        case 0:
                            if (rotateTowards[0].startingDirection)
                                transform.Rotate(rotationSpeed * Time.deltaTime, 0f, 0f);
                            else
                                transform.Rotate(-(rotationSpeed * Time.deltaTime), 0f, 0f);
                        break;
                            
                        case 1:
                            if (rotateTowards[1].startingDirection)
                                transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
                            else
                                transform.Rotate(0f, -(rotationSpeed * Time.deltaTime), 0f);
                        break;

                        case 2:
                            if (rotateTowards[2].startingDirection)
                                transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
                            else
                                transform.Rotate(0f, 0f, -(rotationSpeed * Time.deltaTime));
                        break;
                    }

                    rotateTowards[i].currentTimeToChangeDirection += Time.deltaTime;
                }
            }
        }
    }
}

[System.Serializable]
public class RotateProperties
{
    public bool rotate;
    public bool startingDirection;
    public float timeToChangeDirection;
    public float tmpTimeToChangeDirection;
    public float currentTimeToChangeDirection;
    public bool gottem;
}
