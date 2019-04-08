using System;
using System.IO;
using UnityEngine;
using SimpleJSON;

[Serializable]
public class Character
{
    //General
    public int ID;
    public string username;
    public string nickname;
    public string role;
    public int prestige;

    //Statistics
    public int level;
    public CombatStats stats;

    //Variables
    public bool isOnline;
    public int damageDealt;
    public int gold;
    public int streamstone;
    public int experience;
    public float trainingTimer;
    public bool isTraining;
    public float antiSpamTimer;
    public string[] pvpDuel = new string[2];
    public float pvpDuelTimer;

    //Others
    public int totalTrainingMinute;
    public int pveVictory;
    public int pveDefeat;
    public int pvpVictory;
    public int pvpDefeat;
    public int legendaryCount;
    public int goldReceived;
    public int streamstoneReceived;

    //Equipments Rarity|Tier|Forge|AccessoryType
    public string[] mainHandSlot = new string[3];
    public string[] offHandSlot = new string[3];
    public string[] headSlot = new string[3];
    public string[] chestSlot = new string[3];
    public string[] accessorySlot = new string[4];

    public Character InitNewCharacter(string chatName, int ID, string role)
    {
        Character c = new Character();
        CombatStats stats = new CombatStats();

        c.ID = ID;
        c.username = chatName;
        c.role = role;
        c.prestige = 0;
        
        c.level = 1;
        c.stats = stats;

        c.isOnline = true;
        c.damageDealt = 0;
        c.gold = 100;
        c.streamstone = 1;
        c.experience = 0;
        c.trainingTimer = 0f;
        c.isTraining = false;
        c.antiSpamTimer = 0f;
        c.pvpDuel[0] = "";
        c.pvpDuel[1] = "0";
        c.pvpDuelTimer = 0f;
        
        c.totalTrainingMinute = 0;
        c.pveVictory = 0;
        c.pveDefeat = 0;
        c.pvpVictory = 0;
        c.pvpDefeat = 0;
        c.legendaryCount = 0;
        c.goldReceived = 0;
        c.streamstoneReceived = 0;

        c.mainHandSlot[0] = "Common";
        c.mainHandSlot[1] = "1";
        c.mainHandSlot[2] = "0";
        c.offHandSlot[0] = "Common";
        c.offHandSlot[1] = "1";
        c.offHandSlot[2] = "0";
        c.headSlot[0] = "Common";
        c.headSlot[1] = "1";
        c.headSlot[2] = "0";
        c.chestSlot[0] = "Common";
        c.chestSlot[1] = "1";
        c.chestSlot[2] = "0";
        c.accessorySlot[0] = "Common";
        c.accessorySlot[1] = "1";
        c.accessorySlot[2] = "0";
        c.accessorySlot[3] = "Belt";

        return c;
    }

    public CharacterToJson SaveData()
    {
        CharacterToJson cJson = new CharacterToJson();

        cJson.username = username;
        cJson.role = role;
        cJson.prestige = prestige;
        cJson.ID = ID;
        cJson.level = level;
        cJson.gold = gold;
        cJson.streamstone = streamstone;
        cJson.experience = experience;
        cJson.totalTrainingMinute = totalTrainingMinute;
        cJson.pveVictory = pveVictory;
        cJson.pveDefeat = pveDefeat;
        cJson.pvpVictory = pvpVictory;
        cJson.pvpDefeat = pvpDefeat;
        cJson.legendaryCount = legendaryCount;
        cJson.goldReceived = goldReceived;
        cJson.streamstoneReceived = streamstoneReceived;
        cJson.mainHandSlot[0] = mainHandSlot[0];
        cJson.mainHandSlot[1] = mainHandSlot[1];
        cJson.mainHandSlot[2] = mainHandSlot[2];
        cJson.offHandSlot[0] = offHandSlot[0];
        cJson.offHandSlot[1] = offHandSlot[1];
        cJson.offHandSlot[2] = offHandSlot[2];
        cJson.headSlot[0] = headSlot[0];
        cJson.headSlot[1] = headSlot[1];
        cJson.headSlot[2] = headSlot[2];
        cJson.chestSlot[0] = chestSlot[0];
        cJson.chestSlot[1] = chestSlot[1];
        cJson.chestSlot[2] = chestSlot[2];
        cJson.accessorySlot[0] = accessorySlot[0];
        cJson.accessorySlot[1] = accessorySlot[1];
        cJson.accessorySlot[2] = accessorySlot[2];
        cJson.accessorySlot[3] = accessorySlot[3];

        return cJson;
    }

    public void LoadData()
    {
        if (!File.Exists(Application.persistentDataPath + "/RPG_Stream.json"))
        {
            File.Create(Application.persistentDataPath + "/RPG_Stream.json");
        }
        else
        {
            string path = Application.persistentDataPath + "/RPG_Stream.json";
            string jsonString = File.ReadAllText(path);
            JSONObject charactersData = (JSONObject)JSON.Parse(jsonString);

            for (int i = 0; i < charactersData.Count; i++)
            {
                Character c = new Character();
                c.username = charactersData["Character"][i.ToString()]["username"].Value;
                c.role = charactersData["Character"][i.ToString()]["role"].Value;
                c.prestige = Convert.ToInt32(charactersData["Character"][i.ToString()]["prestige"].Value);
                c.level = Convert.ToInt32(charactersData["Character"][i.ToString()]["level"].Value);
                c.gold = Convert.ToInt32(charactersData["Character"][i.ToString()]["gold"].Value);
                c.streamstone = Convert.ToInt32(charactersData["Character"][i.ToString()]["streamstone"].Value);
                c.experience = Convert.ToInt32(charactersData["Character"][i.ToString()]["experience"].Value);
                c.totalTrainingMinute = Convert.ToInt32(charactersData["Character"][i.ToString()]["totalTrainingMinute"].Value);
                c.pveVictory = Convert.ToInt32(charactersData["Character"][i.ToString()]["pveVictory"].Value);
                c.pveDefeat = Convert.ToInt32(charactersData["Character"][i.ToString()]["pveDefeat"].Value);
                c.pvpVictory = Convert.ToInt32(charactersData["Character"][i.ToString()]["pvpVictory"].Value);
                c.pvpDefeat = Convert.ToInt32(charactersData["Character"][i.ToString()]["pvpDefeat"].Value);
                c.legendaryCount = Convert.ToInt32(charactersData["Character"][i.ToString()]["legendaryCount"].Value);
                c.goldReceived = Convert.ToInt32(charactersData["Character"][i.ToString()]["goldReceived"].Value);
                c.streamstoneReceived = Convert.ToInt32(charactersData["Character"][i.ToString()]["streamstoneReceived"].Value);
                c.mainHandSlot[0] = charactersData["Character"][i.ToString()]["mainHandSlot"][0].Value;
                c.mainHandSlot[1] = charactersData["Character"][i.ToString()]["mainHandSlot"][1].Value;
                c.mainHandSlot[2] = charactersData["Character"][i.ToString()]["mainHandSlot"][2].Value;
                c.offHandSlot[0] = charactersData["Character"][i.ToString()]["offHandSlot"][0].Value;
                c.offHandSlot[1] = charactersData["Character"][i.ToString()]["offHandSlot"][1].Value;
                c.offHandSlot[2] = charactersData["Character"][i.ToString()]["offHandSlot"][2].Value;
                c.headSlot[0] = charactersData["Character"][i.ToString()]["headSlot"][0].Value;
                c.headSlot[1] = charactersData["Character"][i.ToString()]["headSlot"][1].Value;
                c.headSlot[2] = charactersData["Character"][i.ToString()]["headSlot"][2].Value;
                c.chestSlot[0] = charactersData["Character"][i.ToString()]["chestSlot"][0].Value;
                c.chestSlot[1] = charactersData["Character"][i.ToString()]["chestSlot"][1].Value;
                c.chestSlot[2] = charactersData["Character"][i.ToString()]["chestSlot"][2].Value;
                c.accessorySlot[0] = charactersData["Character"][i.ToString()]["accessorySlot"][0].Value;
                c.accessorySlot[1] = charactersData["Character"][i.ToString()]["accessorySlot"][1].Value;
                c.accessorySlot[2] = charactersData["Character"][i.ToString()]["accessorySlot"][2].Value;
                c.accessorySlot[3] = charactersData["Character"][i.ToString()]["accessorySlot"][3].Value;

                c.stats = new CombatStats();
                c.isOnline = false;
                c.damageDealt = 0;
                c.trainingTimer = 0f;
                c.isTraining = false;
                c.antiSpamTimer = 0f;
                c.pvpDuel[0] = "";
                c.pvpDuel[1] = "0";
                c.pvpDuelTimer = 0f;

                GameData.character.Add(c);
            }
        }
    }
}
