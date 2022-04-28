using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TogglingPlatform : MonoBehaviour
{
    public bool startActivated;
    public float intervalsOfActivation;
    public bool canKill;
    public bool dontToggle;
    public bool getRandomInterval;
    public Vector2 randomTime;
    public Material[] materials;
    public Material[] disMaterials;

    bool _realKill;

    void Start()
    {   
        if (!dontToggle)
        {
            PlatformSetting(startActivated ? true : false);

            if (startActivated) StartCoroutine(ActivatePlatform());
            else StartCoroutine(DeactivatePlatform());
        }

        else
            _realKill = true;
    }

    IEnumerator ActivatePlatform()
    {
        PlatformSetting(true);
        _realKill = true;
        yield return new WaitForSeconds(!getRandomInterval ? intervalsOfActivation : Random.Range(randomTime.x, randomTime.y));
        StartCoroutine(DeactivatePlatform());
    }

    IEnumerator DeactivatePlatform()
    {
        PlatformSetting(false);
        _realKill = false;
        yield return new WaitForSeconds(!getRandomInterval ? intervalsOfActivation : Random.Range(randomTime.x, randomTime.y));
        StartCoroutine(ActivatePlatform());
    }

    void PlatformSetting(bool _ac)
    {
        GetComponent<Collider>().enabled = _ac;

        if (GetComponent<MeshRenderer>().materials.Length == 1)
            GetComponent<MeshRenderer>().material = _ac ? materials[0] : materials[1];
        else
            GetComponent<MeshRenderer>().materials = _ac ? materials : disMaterials;
    }

    void OnTriggerEnter(Collider other) 
    {
        if (canKill && _realKill)
        {
            if (other.CompareTag("Player"))
                other.GetComponent<PlayerController>().Respawn();
        }   
    }
}
