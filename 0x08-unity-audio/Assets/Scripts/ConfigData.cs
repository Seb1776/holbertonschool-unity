using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConfigData
{
    public bool invertY;
    public float sfxVol, musVol;

    public ConfigData (OptionsMenu pc)
    {
        invertY = pc.invertConfigY;
        sfxVol = pc.sfxVol;
        musVol = pc.musVol;
    }
}