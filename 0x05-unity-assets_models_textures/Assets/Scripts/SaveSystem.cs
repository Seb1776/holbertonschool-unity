using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
    public static void SaveTime(Timer time)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/bsttms.run";
        FileStream stream = new FileStream(path, FileMode.Create);

        TimesData td = new TimesData(time);
        formatter.Serialize(stream, td);
        stream.Close();
    }

    public static TimesData LoadTimes()
    {
        string path = Application.persistentDataPath + "/bsttms.run";

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            TimesData td = formatter.Deserialize(stream) as TimesData;
            stream.Close();

            return td;
        }

        return null;
    }
}
