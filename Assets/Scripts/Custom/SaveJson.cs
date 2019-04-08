using UnityEngine;
using System;
using System.IO;

public class SaveJson
{
    public static string load = "";
    public string AddNewCharacterToJson()
    {
        string json = "{\"Character\": {";
        foreach (Character c in GameData.character)
        {
            json += "\"" + c.ID + "\":";
            json += JsonUtility.ToJson(c.SaveData());
            json += ",";
        }
        json = json.Remove(json.Length - 1);
        json = json.Insert(json.Length, "}}");

        Debug.Log("Json saved-> " + json.ToString());
        return json.ToString();
    }

    public string GetJson()
    {
        return load;
    }
}
