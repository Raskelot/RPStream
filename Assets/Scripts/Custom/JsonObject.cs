using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class JsonObject
{
    public string _links;
    public int chatter_count;
    public Chatters chatters;

    public static JsonObject CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<JsonObject>(jsonString);
    }
}

[Serializable]
public class Chatters
{
    public string[] moderators;
    public string[] staff;
    public string[] admins;
    public string[] global_mods;
    public string[] vips;
    public string[] viewers;
    public string[] broadcaster;
}

