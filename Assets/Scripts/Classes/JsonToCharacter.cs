using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class JsonToCharacter
{
    public string username;
    public string role;
    public int ID;
    public int prestige;
    public int level;
    public int gold;
    public int streamstone;
    public int experience;
    public int totalTrainingMinute;
    public int pveVictory;
    public int pveDefeat;
    public int pvpVictory;
    public int pvpDefeat;
    public int legendaryCount;
    public int goldReceived;
    public int materialsReceived;
    public string[] mainHandSlot = new string[3];
    public string[] offHandSlot = new string[3];
    public string[] headSlot = new string[3];
    public string[] chestSlot = new string[3];
    public string[] accessorySlot = new string[4];
    
    
}
