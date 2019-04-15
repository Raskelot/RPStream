using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.IO;
using UnityEngine.UI;
using System.Net;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ChatCommand : MonoBehaviour
{
    private TcpClient twitchClient;
    private TcpClient twitchClientBot;
    private StreamReader reader;
    private StreamWriter writer;
    private StreamWriter writerBot;
    private string username = "", password = "", channelName = ""; //Get the password from http://twitchapps.com/tmi
    private string botUsername = "bot_rpg", botPassword = "oauth:lc3hiheve28tlqgx8taqjnfen380mf";

    private float reconnectTimer;
    private float goldWatchingTimer;
    private float mapSpawnTimer = 30f;
    
    LoadViewerList ViewerList = new LoadViewerList();
    SaveJson saveJson = new SaveJson();
    CombatSystem cs = new CombatSystem();

    void Start()
    {
        cs = GameObject.Find("Battle Info").GetComponent<CombatSystem>();
        //Connect();
        gameObject.AddComponent<LoadViewerList>();

        Character addCharacter = new Character();
        addCharacter.LoadData();
        /*GameData.character.Add(addCharacter.InitNewCharacter("knight", GameData.character.Count, "Knight"));
        GameData.character.Add(addCharacter.InitNewCharacter("warrior", GameData.character.Count, "Warrior"));
        GameData.character.Add(addCharacter.InitNewCharacter("archer", GameData.character.Count, "Archer"));
        GameData.character.Add(addCharacter.InitNewCharacter("thief", GameData.character.Count, "Thief"));
        GameData.character.Add(addCharacter.InitNewCharacter("wizard", GameData.character.Count, "Wizard"));
        GameData.character.Add(addCharacter.InitNewCharacter("priest", GameData.character.Count, "Priest"));
        GameData.character.Add(addCharacter.InitNewCharacter("priest2", GameData.character.Count, "Priest"));
        GameData.character.Add(addCharacter.InitNewCharacter("thief3", GameData.character.Count, "Thief"));
        GameData.character.Add(addCharacter.InitNewCharacter("thief2", GameData.character.Count, "Thief"));
        GameData.character.Add(addCharacter.InitNewCharacter("priest5", GameData.character.Count, "Priest"));
        GameData.character.Add(addCharacter.InitNewCharacter("warrior2", GameData.character.Count, "Warrior"));*/

        /*foreach (Character c in GameData.character)
        {
            GameData.characterJoined.Add(c);
        }*/
    }

    void Update()
    {
        if (username != "")
        {
            AutoReconnect();

            if (GameData.character.Count > 0)
            {
                GoldPerMinuteWatching();

                foreach (Character c in GameData.character)
                {
                    if (c.antiSpamTimer > 0)
                    {
                        c.antiSpamTimer -= Time.deltaTime;
                    }

                    if (c.pvpDuelTimer > 0)
                    {
                        c.pvpDuelTimer -= Time.deltaTime;
                    }

                    //Training timers
                    if (c.isTraining)
                    {
                        c.trainingTimer -= Time.deltaTime;
                        if (c.trainingTimer <= 0)
                        {
                            c.trainingTimer = GameData.trainingTick;
                            c.experience += GameData.trainingExperience;
                            c.totalTrainingMinute += (int)(GameData.trainingTick / 60.0f);
                        }
                    }

                    //No experience gain if max level
                    if (c.level == 100)
                    {
                        c.experience = 0;
                    }

                    //Leveling up
                    if (c.experience >= 100)
                    {
                        c.level++;
                        c.experience -= 100;
                        SendTwitchMessage(String.Format("{0} has leveled up!", c.username));
                    }

                    /*if (c.pvpDuelTimer <= 0)MAKE BUGS
                    {
                        GameData.character[GetCharacterIndex(c.username)].pvpDuelTimer = 0;
                        GameData.character[GetCharacterIndex(c.username)].pvpDuel[0] = "";
                        GameData.character[GetCharacterIndex(c.username)].pvpDuel[1] = "0";
                    }*/
                }
            }

            mapSpawnTimer -= Time.deltaTime;
            if (mapSpawnTimer <= 0)
            {
                GameData.showMap = true;
            }

            if (!GameObject.Find("Battle Info").GetComponent<CombatSystem>().isFighting)
            {
                if (GameData.mapTimer < 0)
                {
                    GameData.mapTimer = 0;
                }
                else
                {
                    GameData.mapTimer -= Time.deltaTime;
                }
            }

            if (GameData.mapTimer <= 0 && GameData.characterJoined.Count > 0)
            {
                GameData.mapTimer = 600f;

                if (GameData.voteNorth != null && GameData.voteEast != null && GameData.voteSouth != null && GameData.voteWest != null)
                {
                    if (GameData.voteNorth.Count > GameData.voteEast.Count &&
                        GameData.voteNorth.Count > GameData.voteSouth.Count &&
                        GameData.voteNorth.Count > GameData.voteWest.Count)
                    {
                        GameData.mapPosition.y++;
                    }
                    else
                    if (GameData.voteEast.Count > GameData.voteSouth.Count &&
                        GameData.voteEast.Count > GameData.voteWest.Count)
                    {
                        GameData.mapPosition.x++;
                    }
                    else
                    if (GameData.voteSouth.Count > GameData.voteWest.Count)
                    {
                        GameData.mapPosition.y--;
                    }
                    else
                    {
                        if ((int)GameData.mapPosition.x > 0)
                        {
                            if (GameData.map[(int)GameData.mapPosition.x - 1, (int)GameData.mapPosition.y] != MapEvent.None &&
                                GameData.map[(int)GameData.mapPosition.x - 1, (int)GameData.mapPosition.y] != MapEvent.Success &&
                                GameData.map[(int)GameData.mapPosition.x - 1, (int)GameData.mapPosition.y] != MapEvent.City &&
                                GameData.map[(int)GameData.mapPosition.x - 1, (int)GameData.mapPosition.y] != MapEvent.Failure)
                            {
                                GameData.mapPosition.x--;
                            }
                            else if ((int)GameData.mapPosition.x < 7)
                            {
                                if (GameData.map[(int)GameData.mapPosition.x + 1, (int)GameData.mapPosition.y] != MapEvent.None &&
                                    GameData.map[(int)GameData.mapPosition.x + 1, (int)GameData.mapPosition.y] != MapEvent.Success &&
                                    GameData.map[(int)GameData.mapPosition.x + 1, (int)GameData.mapPosition.y] != MapEvent.City &&
                                    GameData.map[(int)GameData.mapPosition.x + 1, (int)GameData.mapPosition.y] != MapEvent.Failure)
                                {
                                    GameData.mapPosition.x++;
                                }
                                else if ((int)GameData.mapPosition.y > 0)
                                {
                                    if (GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y - 1] != MapEvent.None &&
                                        GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y - 1] != MapEvent.Success &&
                                        GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y - 1] != MapEvent.City &&
                                        GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y - 1] != MapEvent.Failure)
                                    {
                                        GameData.mapPosition.y--;
                                    }
                                    else if ((int)GameData.mapPosition.y < 7)
                                    {
                                        if (GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y + 1] != MapEvent.None &&
                                            GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y + 1] != MapEvent.Success &&
                                            GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y + 1] != MapEvent.City &&
                                            GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y + 1] != MapEvent.Failure)
                                        {
                                            GameData.mapPosition.y++;
                                        }
                                        else
                                        {
                                            GameObject.Find("MiniMap").GetComponent<UI>().InitialMap();
                                            SendTwitchMessage(String.Format("You are now in The City (3, 3). !craft and !forge are now available."));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if ((int)GameData.mapPosition.x < 7)
                            {
                                if (GameData.map[(int)GameData.mapPosition.x + 1, (int)GameData.mapPosition.y] != MapEvent.None &&
                                    GameData.map[(int)GameData.mapPosition.x + 1, (int)GameData.mapPosition.y] != MapEvent.Success &&
                                    GameData.map[(int)GameData.mapPosition.x + 1, (int)GameData.mapPosition.y] != MapEvent.City &&
                                    GameData.map[(int)GameData.mapPosition.x + 1, (int)GameData.mapPosition.y] != MapEvent.Failure)
                                {
                                    GameData.mapPosition.x++;
                                }
                                else if ((int)GameData.mapPosition.y > 0)
                                {
                                    if (GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y - 1] != MapEvent.None &&
                                        GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y - 1] != MapEvent.Success &&
                                        GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y - 1] != MapEvent.City &&
                                        GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y - 1] != MapEvent.Failure)
                                    {
                                        GameData.mapPosition.y--;
                                    }
                                    else if ((int)GameData.mapPosition.y < 7)
                                    {
                                        if (GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y + 1] != MapEvent.None &&
                                            GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y + 1] != MapEvent.Success &&
                                            GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y + 1] != MapEvent.City &&
                                            GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y + 1] != MapEvent.Failure)
                                        {
                                            GameData.mapPosition.y++;
                                        }
                                        else
                                        {
                                            GameObject.Find("MiniMap").GetComponent<UI>().InitialMap();
                                            SendTwitchMessage(String.Format("You are now in The City (3, 3). !craft and !forge are now available."));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                GameObject.Find("MiniMap").GetComponent<UI>().InitialMap();
                                SendTwitchMessage(String.Format("You are now in The City (3, 3). !craft and !forge are now available."));
                            }
                        }
                    }
                    LaunchEvent();
                }

                GameData.voteNorth.Clear();
                GameData.voteEast.Clear();
                GameData.voteSouth.Clear();
                GameData.voteWest.Clear();
            }
        }
    }

    public void Login()
    {
        username = GameObject.Find("Username").transform.GetChild(2).GetComponent<Text>().text;
        password = GameObject.Find("Oauth").transform.GetChild(2).GetComponent<Text>().text;
        channelName = username;
        Connect();
        GameData.showMap = true;
        Destroy(GameObject.Find("Username").gameObject);
        Destroy(GameObject.Find("Oauth").gameObject);
        Destroy(GameObject.Find("HowToLogin").gameObject);
        Destroy(GameObject.Find("Connect").gameObject);
        Destroy(GameObject.Find("Title").gameObject);
    }

    public void HowToLogin()
    {
        Application.OpenURL("http://twitchapps.com/tmi");
    }

    private void AutoReconnect()
    {
        if (username != "")
        {
            reconnectTimer += Time.deltaTime;
            if (reconnectTimer >= 60)
            {
                Connect();
                reconnectTimer = 0;
            }
            ReadChat();
        }
    }

    public void Connect()
    {
        if (username != "")
        {
            twitchClient = new TcpClient("irc.chat.twitch.tv", 6667);
            reader = new StreamReader(twitchClient.GetStream());
            writer = new StreamWriter(twitchClient.GetStream());

            writer.WriteLine("PASS " + password);
            writer.WriteLine("NICK " + username.ToLower());
            writer.WriteLine("USER " + username.ToLower() + " 8 * :" + username.ToLower());
            writer.WriteLine("JOIN #" + channelName.ToLower());
            writer.Flush();
        }
    }

    public void SendTwitchMessage(string msg)
    {
        twitchClientBot = new TcpClient("irc.chat.twitch.tv", 6667);
        writerBot = new StreamWriter(twitchClientBot.GetStream());

        writerBot.WriteLine("PASS " + botPassword);
        writerBot.WriteLine("NICK " + botUsername.ToLower());
        writerBot.WriteLine("USER " + botUsername.ToLower() + " 8 * :" + botUsername.ToLower());
        writerBot.WriteLine("JOIN #" + channelName.ToLower());
        if (msg[0] == '/')
            writerBot.WriteLine("PRIVMSG #" + channelName + " :" + msg);
        else
            writerBot.WriteLine("PRIVMSG #" + channelName + " :/me : " + msg);
        writerBot.Flush();
    }

    private void ReadChat()
    {
        if (twitchClient.Available > 0)
        {
            var message = reader.ReadLine();

            if (message.Contains("PRIVMSG"))
            {
                var splitPoint = message.IndexOf("!", 1);
                var chatName = message.Substring(0, splitPoint);
                chatName = chatName.Substring(1);

                splitPoint = message.IndexOf(":", 1);
                message = message.Substring(splitPoint + 1);

                //Debug.Log(String.Format("{0}: {1}", chatName, message));

                Commands(chatName, message);
            }
        }
    }

    private void Commands(string chatName, string message)
    {
        CreateNewCharacter(chatName, message);

        if (IsCharacterExist(chatName) && !cs.isFighting)
        {
            Stats(chatName, message);
            Donate(chatName, message);
            Train(chatName, message);
            Join(chatName, message);
            Nickname(chatName, message);
            MapDirection(chatName, message);
            //Prestige(chatName, message);
            //Duel(chatName, message);//if player is online only - 60s to accept
            //Accept(chatName, message);
            //Decline(chatName, message);

            Classes(chatName, message);
            Knight(chatName, message);
            Warrior(chatName, message);
            Thief(chatName, message);
            Archer(chatName, message);
            Wizard(chatName, message);
            Priest(chatName, message);

            if (GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y] == MapEvent.City)
            {
                Craft(chatName, message);
                Forge(chatName, message);
            }
            
            //Admin commands
            if (chatName == channelName || channelName == "raskelot")
            {
                SetGold(chatName, message);
                SetStreamstone(chatName, message);
                SetExperience(chatName, message);
                SetLevel(chatName, message);
                SetSpamTimer(chatName, message);

                ShowMap(chatName, message);
                HideMap(chatName, message);
            }
        }
    }

    private void Stats(string username, string message)
    {
        if (message.ToLower() == "!stats")
        {
            Character c = GameData.character[GetCharacterIndex(username)];
            if (c.antiSpamTimer <= 0f)
            {
                SendTwitchMessage(GetInspectText(username, c));
                SendTwitchMessage(GetInventoryText(username, c));
                c.antiSpamTimer = GameData.spamTimer;
            }
        }
        else
        {
            if (message.ToLower().Split(' ')[0] == "!stats" && message.Split(' ').Length == 2)
            {
                Character c = GameData.character[GetCharacterIndex(username)];
                Character t = GameData.character[GetCharacterIndex(message.ToLower().Split(' ')[1].Replace("@", ""))];

                if (c.antiSpamTimer <= 0f)
                {
                    if (t.username != "")
                    {
                        c.antiSpamTimer = GameData.spamTimer;
                        SendTwitchMessage(GetInspectText(username, t));
                        SendTwitchMessage(GetInventoryText(username, t));
                    }
                }
            }
        }
    }

    private void Prestige(string username, string message)
    {
        if (message.ToLower() == "!prestige")
        {
            Character c = GameData.character[GetCharacterIndex(username)];

            if (c.level == 100)
            {
                PrestigeResetStats(c);
                SendTwitchMessage(String.Format("{0} has prestige to {1}*!", username, c.prestige.ToString()));
            }
        }
    }
    

    private void Donate(string username, string message)
    {
        if ((message.ToLower().Split(' ')[0] == "!donate" || message.ToLower().Split(' ')[0] == "!don") && message.ToLower().Split(' ')[1].Replace("@", "") != username && message.Split(' ').Length == 3)
        {
            int to = -1;
            int from = -1;
            int goldAmount = Convert.ToInt32(message.ToLower().Split(' ')[2]);

            from = GameData.character[GetCharacterIndex(username)].ID;
            if (IsCharacterExist(message.ToLower().Split(' ')[1].Replace("@", "")))
            {
                to = GameData.character[GetCharacterIndex(message.ToLower().Split(' ')[1].Replace("@", ""))].ID;
            }

            if (GameData.character[from].gold >= goldAmount && to != -1 && goldAmount > 0 && GameData.character[to].gold + goldAmount <= GameData.maxGold)
            {
                GameData.character[from].gold -= goldAmount;
                GameData.character[to].gold += goldAmount;
                SendTwitchMessage(String.Format("{0} is giving {1} gold to {2} for a total of {3} gold.",
                    GameData.character[from].username, goldAmount, GameData.character[to].username, GameData.character[to].gold));
            }
        }
    }

    private void Train(string username, string message)
    {
        if ((message.ToLower() == "!train" || message.ToLower() == "!afk") && message.Split(' ').Length == 1)
        {
            Character c = GameData.character[GetCharacterIndex(username)];
            
            if (!c.isTraining)
            {
                c.isTraining = true;
                c.trainingTimer = GameData.trainingTick;
                SendTwitchMessage(String.Format("/w {0} You are training, earning {1} exp every {2} minutes, but cannot join any event.", c.username, GameData.trainingExperience, GameData.trainingTick / 60));
                foreach (Character character in GameData.characterJoined)
                {
                    if (character.username == c.username)
                    {
                        GameData.characterJoined.Remove(character);
                    }
                }
            }
            else
            {
                c.isTraining = false;
                SendTwitchMessage(String.Format("/w {0} You are no longer training.", c.username));
            }
        }
    }

    private void Classes(string username, string message)
    {
        if (message.ToLower() == "!class" && message.Split(' ').Length == 1)
        {
            Character c = GameData.character[GetCharacterIndex(username)];
            if (c.antiSpamTimer <= 0)
            {
                c.antiSpamTimer = GameData.spamTimer;
                SendTwitchMessage(String.Format("/w {0} List of classes -> !knight, !warrior, !thief, !archer, !wizard and !priest.", c.username));
            }
        }
    }

    private void SetGold(string username, string message)
    {
        if (message.ToLower().Split(' ')[0] == "!setgold")
        {
            string target = message.ToLower().Split(' ')[1].Replace("@", "");
            int gold = Convert.ToInt32(message.ToLower().Split(' ')[2]);

            if ((gold > 0 && gold <= GameData.maxGold) && IsCharacterExist(target))
            {
                GameData.character[GetCharacterIndex(target)].gold = gold;
                SendTwitchMessage(String.Format("Admin Command -> {0}'s gold is set to {1}.", target, gold));
            }
        }
    }

    private void SetStreamstone(string username, string message)
    {
        if (message.ToLower().Split(' ')[0] == "!setstreamstone")
        {
            string target = message.ToLower().Split(' ')[1].Replace("@", "");
            int streamstone = Convert.ToInt32(message.ToLower().Split(' ')[2]);

            if ((streamstone > 0 && streamstone <= GameData.maxStreamstone) && IsCharacterExist(target))
            {
                GameData.character[GetCharacterIndex(target)].streamstone = streamstone;
                SendTwitchMessage(String.Format("Admin Command -> {0}'s streamstone is set to {1}.", target, streamstone));
            }
        }
    }

    private void SetExperience(string username, string message)
    {
        if (message.ToLower().Split(' ')[0] == "!setexp")
        {
            string target = message.ToLower().Split(' ')[1].Replace("@", "");
            int experience = Convert.ToInt32(message.ToLower().Split(' ')[2]);

            if ((experience >= 0 && experience <= 100) && IsCharacterExist(target))
            {
                GameData.character[GetCharacterIndex(target)].experience = experience;
                SendTwitchMessage(String.Format("Admin Command -> {0}'s experience is set to {1}.", target, experience));
            }
        }
    }

    private void SetLevel(string username, string message)
    {
        if (message.ToLower().Split(' ')[0] == "!setlevel")
        {
            string target = message.ToLower().Split(' ')[1].Replace("@", "");
            int level = Convert.ToInt32(message.ToLower().Split(' ')[2]);

            if ((level > 0 && level <= 100) && IsCharacterExist(target))
            {
                GameData.character[GetCharacterIndex(target)].level = level;
                SendTwitchMessage(String.Format("Admin Command -> {0}'s level is set to {1}.", target, level));
            }
        }
    }

    private void SetSpamTimer(string username, string message)
    {
        if (message.ToLower().Split(' ')[0] == "!setspamtimer")
        {
            string stringTimer = message.ToLower().Split(' ')[1];
            int timer = Convert.ToInt32(stringTimer);
            
            if (timer > 0)
            {
                GameData.spamTimer = timer;
                SendTwitchMessage(String.Format("Spam timer is now set to {0}s.", timer));
            }
        }
    }

    private void PrestigeResetStats(Character c)
    {
        c.prestige++;
        c.level = 1;
        c.experience = 0;
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
    }

    public int GetCharacterIndex(string username)
    {
        foreach (Character c in GameData.character)
        {
            if (username == c.username)
            {
                return c.ID;
            }
        }
        return -1;
    }

    private string GetInspectText(string username, Character c)
    {
        int totalHP = (c.level * 20) + GetHeadPowerValue(c) + GetChestPowerValue(c);
        int totalPower = (c.level * 5) + GetMainHandPowerValue(c);
        int totalEvasion = 0;
        int totalCritical = 0;

        switch (c.role)
        {
            case "Knight":
                totalEvasion += GetOffHandPowerValue(c);
                break;
            case "Warrior":
            case "Wizard":
                totalPower += GetOffHandPowerValue(c);
                break;
            case "Thief":
            case "Archer":
                totalCritical += GetOffHandPowerValue(c);
                break;
        }

        switch (c.accessorySlot[3])
        {
            case "Mantle":
                totalEvasion += GetMantlePowerValue(c);
                break;
            case "Amulet":
                totalHP += GetMantlePowerValue(c);
                break;
            case "Ring":
                totalCritical += GetRingPowerValue(c);
                break;
            case "Belt":
                totalPower += GetBeltPowerValue(c);
                break;
        }
        string prestige = c.prestige > 0 ? " " + (c.prestige > 1 ? c.prestige.ToString() + "*" : " *") : "";
        return string.Format("/w {14} {0}{1} {13}, Lvl: {2} {3} has {4} HP, {5} Power, {6} ({7}%) Critical, {8} ({9}%) Evasion, {10} Gold, {11} Streamstone and {12}% experiences.",
                            c.username, prestige, c.level, c.role, totalHP, totalPower, totalCritical, GetPercentStats(totalCritical).ToString(), totalEvasion, GetPercentStats(totalEvasion).ToString(), c.gold, c.streamstone, c.experience, c.nickname, username);
    }

    private string GetInventoryText(string username, Character c)
    {
        string offHandValue = "";
        switch (c.role)
        {
            case "Knight":
                offHandValue = "Evasion: ";
                break;
            case "Warrior":
                offHandValue = "Power: ";
                break;
            case "Thief":
            case "Archer":
                offHandValue = "Critical: ";
                break;
            case "Priest":
                offHandValue = "Heal Power: ";
                break;
        }
        offHandValue += GetOffHandPowerValue(c).ToString();

        string accessory = ": ";
        switch (c.accessorySlot[3])
        {
            case "Mantle":
                accessory += GetMantlePowerValue(c);
                break;
            case "Amulet":
                accessory += GetAmuletPowerValue(c);
                break;
            case "Ring":
                accessory += GetRingPowerValue(c);
                break;
            case "Belt":
                accessory += GetBeltPowerValue(c);
                break;
        }

        return string.Format("/w {29} {0}{1} {28}, has {2} {3}{4} tier {5} Power: {6}, {7} {8}{9} tier {10} {11}, {12} {13}{14} tier {15} HP: {16}, {17} {18}{19} tier {20} HP: {21}, {22} {23}{24} tier {25}.",
                            c.username, c.prestige > 0 ? " *" : "", c.mainHandSlot[0], GetMainHandName(c.role), Convert.ToInt32(c.mainHandSlot[2]) > 0 ? " (+" + c.mainHandSlot[2] + ")" : "", c.mainHandSlot[1], GetMainHandPowerValue(c),
                            c.offHandSlot[0], GetOffHandName(c.role), Convert.ToInt32(c.offHandSlot[2]) > 0 ? " (+" + c.offHandSlot[2] + ")" : "", c.offHandSlot[1], offHandValue,
                            c.headSlot[0], GetHeadName(c.role), Convert.ToInt32(c.headSlot[2]) > 0 ? " (+" + c.headSlot[2] + ")" : "", c.headSlot[1], GetHeadPowerValue(c),
                            c.chestSlot[0], GetChestName(c.role), Convert.ToInt32(c.chestSlot[2]) > 0 ? " (+" + c.chestSlot[2] + ")" : "", c.chestSlot[1], GetChestPowerValue(c),
                            c.accessorySlot[0], c.accessorySlot[3], Convert.ToInt32(c.accessorySlot[2]) > 0 ? " (+" + c.accessorySlot[2] + ")" : "", c.accessorySlot[1], c.accessorySlot[3], accessory,
                            c.nickname, username);
    }

    private void Duel(string username, string message)
    {
        if (message.ToLower().Split(' ')[0] == "!duel")
        {
            Character c = GameData.character[GetCharacterIndex(username)];
            string target = message.ToLower().Split(' ')[1];
            int amount = Convert.ToInt32(message.ToLower().Split(' ')[2]);

            if (IsCharacterExist(target) && amount > 0)
            {
                if (GameData.character[GetCharacterIndex(username)].pvpDuel[0] == "")
                {
                    if (GameData.character[GetCharacterIndex(target)].gold >= amount)
                    {
                        GameData.character[GetCharacterIndex(target)].pvpDuel[0] = username;
                        GameData.character[GetCharacterIndex(target)].pvpDuel[1] = amount.ToString();
                        GameData.character[GetCharacterIndex(target)].pvpDuelTimer = GameData.pvpDuelTimer;

                        SendTwitchMessage(String.Format("{0} level {1} is challenging {2} level {3} to a duel. !Accept or it will auto !Decline in {4} seconds.",
                            username, c.level, target, GameData.character[GetCharacterIndex(target)].level, GameData.pvpDuelTimer));

                        Accept("toleksar", "!accept");
                    }
                    else
                    {
                        SendTwitchMessage(String.Format("{0} doesn't have the {1} gold to duel.", target, amount));
                    }
                }
            }
        }
    }

    private void Accept(string username, string message)
    {
        if (message.ToLower() == "!accept")
        {
            Character c = GameData.character[GetCharacterIndex(username)];

            if (c.pvpDuel[0] != "")
            {
                string target = GameData.character[GetCharacterIndex(username)].pvpDuel[0];
                string cost = GameData.character[GetCharacterIndex(username)].pvpDuel[1];

                //Inspect(username, "!inspect");
                //Inspect(target, "!inspect");

                //Duel calculation

                //+cost to winner, -cost to loser
            }
        }
    }

    private void Decline(string username, string message)
    {
        if (message.ToLower() == "!decline")
        {
            Character c = GameData.character[GetCharacterIndex(username)];

            if (c.pvpDuel[0] != "")
            {
                SendTwitchMessage(String.Format("{0} declined the duel against {1}.", username, c.pvpDuel[0]));

                GameData.character[GetCharacterIndex(c.username)].pvpDuelTimer = 0;
                GameData.character[GetCharacterIndex(c.username)].pvpDuel[0] = "";
                GameData.character[GetCharacterIndex(c.username)].pvpDuel[1] = "0";
            }
        }
    }

    private void Nickname(string username, string message)
    {
        if (message.ToLower().Split(' ')[0] == "!nickname" && message.Split(' ').Length == 2)
        {
            Character c = GameData.character[GetCharacterIndex(username)];
            c.nickname = "(" + message.Split(' ')[1] + ")";
            SendTwitchMessage(String.Format("/w {0} Your nickname has changed to {1}.", c.username, message.Split(' ')[1]));
        }
    }

    private void CreateNewCharacter(string username, string message)
    {
        if (message.ToLower() == "!create")
        {
            if (!IsCharacterExist(username))
            {
                Character addCharacter = new Character();
                
                GameData.character.Add(addCharacter.InitNewCharacter(username, GameData.character.Count, "Warrior"));
                saveJson.AddNewCharacterToJson();

                SendTwitchMessage(String.Format("{0} just joined Avaron's realm, welcome!", username));
                SendTwitchMessage(String.Format("/w {0} Welcome, you can change your class -> !knight, !warrior, !thief, !archer, !wizard and !priest.", username));
                Save();
            }
        }
        else
        {
            if (message.ToLower().Split(' ')[0] == "!create")
            {
                string role = FirstLetterToUpper(message.ToLower().Split(' ')[1]);

                if (!IsCharacterExist(username))
                {
                    Character addCharacter = new Character();

                    if (role == "Knight" || role == "Warrior" || role == "Thief" || role == "Archer" || role == "Wizard" || role == "Priest")
                    {
                        GameData.character.Add(addCharacter.InitNewCharacter(username, GameData.character.Count, role));
                    }
                    else
                    {
                        GameData.character.Add(addCharacter.InitNewCharacter(username, GameData.character.Count, "Warrior"));
                    }
                    saveJson.AddNewCharacterToJson();
                    SendTwitchMessage(String.Format("{0} just joined Avaron's realm, welcome!", username));
                    SendTwitchMessage(String.Format("/w {0} Welcome, you can change your class -> !knight, !warrior, !thief, !archer, !wizard and !priest.", username));
                    Save();
                }
            }
        }
    }

    private void Knight(string username, string message)
    {
        if ((message.ToLower() == "!knight" || message.ToLower() == "!chevalier") && message.Split(' ').Length == 1)
        {
            Character c = GameData.character[GetCharacterIndex(username)];

            if (c.role != "Knight" && c.antiSpamTimer <= 0)
            {
                c.role = "Knight";
                c.antiSpamTimer = GameData.spamTimer;
                SendTwitchMessage(String.Format("/w {0} You are now a Knight!", username));
            }
        }
    }

    private void Warrior(string username, string message)
    {
        if ((message.ToLower() == "!warrior" || message.ToLower() == "!guerrier") && message.Split(' ').Length == 1)
        {
            Character c = GameData.character[GetCharacterIndex(username)];

            if (c.role != "Warrior" && c.antiSpamTimer <= 0)
            {
                c.role = "Warrior";
                c.antiSpamTimer = GameData.spamTimer;
                SendTwitchMessage(String.Format("/w {0} You are now a Warrior!", username));
            }
        }
    }

    private void Thief(string username, string message)
    {
        if ((message.ToLower() == "!thief" || message.ToLower() == "!voleur") && message.Split(' ').Length == 1)
        {
            Character c = GameData.character[GetCharacterIndex(username)];

            if (c.role != "Thief" && c.antiSpamTimer <= 0)
            {
                c.role = "Thief";
                c.antiSpamTimer = GameData.spamTimer;
                SendTwitchMessage(String.Format("/w {0} You are now a Thief!", username));
            }
        }
    }

    private void Archer(string username, string message)
    {
        if (message.ToLower() == "!archer" && message.Split(' ').Length == 1)
        {
            Character c = GameData.character[GetCharacterIndex(username)];

            if (c.role != "Archer" && c.antiSpamTimer <= 0)
            {
                c.role = "Archer";
                c.antiSpamTimer = GameData.spamTimer;
                SendTwitchMessage(String.Format("/w {0} You are now an Archer!", username));
            }
        }
    }

    private void Wizard(string username, string message)
    {
        if ((message.ToLower() == "!wizard" || message.ToLower() == "!magicien" || message.ToLower() == "!harry") && message.Split(' ').Length == 1)
        {
            Character c = GameData.character[GetCharacterIndex(username)];

            if (c.role != "Wizard" && c.antiSpamTimer <= 0)
            {
                c.role = "Wizard";
                c.antiSpamTimer = GameData.spamTimer;
                SendTwitchMessage(String.Format("/w {0} You are now a Wizard! ...Harry", username));
            }
        }
    }

    private void Priest(string username, string message)
    {
        if ((message.ToLower() == "!priest" || message.ToLower() == "!prêtre") && message.Split(' ').Length == 1)
        {
            Character c = GameData.character[GetCharacterIndex(username)];

            if (c.role != "Priest" && c.antiSpamTimer <= 0)
            {
                c.role = "Priest";
                c.antiSpamTimer = GameData.spamTimer;
                SendTwitchMessage(String.Format("/w {0} You are now a Priest!", username));
            }
        }
    }

    private void MapDirection(string username, string message)
    {
        if ((message.ToLower() == "!north" || message.ToLower() == "!nord") && message.Split(' ').Length == 1)
        {
            if (!CharacterHAsVoted(username) && GameData.mapPosition.y < 6)
            {
                GameData.voteNorth.Add(username);
            }
        }

        if ((message.ToLower() == "!east" || message.ToLower() == "!est") && message.Split(' ').Length == 1)
        {
            if (!CharacterHAsVoted(username) && GameData.mapPosition.x < 6)
            {
                GameData.voteEast.Add(username);
            }
        }

        if ((message.ToLower() == "!south" || message.ToLower() == "!sud") && message.Split(' ').Length == 1)
        {
            if (!CharacterHAsVoted(username) && GameData.mapPosition.y > 0)
            {
                GameData.voteSouth.Add(username);
            }
        }

        if ((message.ToLower() == "!west" || message.ToLower() == "!ouest") && message.Split(' ').Length == 1)
        {
            if (!CharacterHAsVoted(username) && GameData.mapPosition.x > 0)
            {
                GameData.voteWest.Add(username);
            }
        }
    }

    private bool CharacterHAsVoted(string username)
    {
        bool hasVoted = false;
        if (GameData.voteNorth != null)
        {
            foreach (string c in GameData.voteNorth)
            {
                if (c == username)
                {
                    hasVoted = true;
                }
            }
        }
        if (GameData.voteEast != null)
        {
            foreach (string c in GameData.voteEast)
            {
                if (c == username)
                {
                    hasVoted = true;
                }
            }
        }
        if (GameData.voteSouth != null)
        {
            foreach (string c in GameData.voteSouth)
            {
                if (c == username)
                {
                    hasVoted = true;
                }
            }
        }
        if (GameData.voteWest != null)
        {
            foreach (string c in GameData.voteWest)
            {
                if (c == username)
                {
                    hasVoted = true;
                }
            }
        }
        return hasVoted;
    }

    private void ShowMap(string username, string message)
    {
        if (message.ToLower() == "!showmap" && username == channelName.ToLower())
        {
            GameData.showMap = true;
        }
    }

    private void HideMap(string username, string message)
    {
        if (message.ToLower() == "!hidemap" && username == channelName.ToLower())
        {
            GameData.showMap = false;
        }
    }

    public void DropGear(string username)
    {
        int dropChance = -1;
        switch (cs.boss.type)
        {
            case MapEvent.Explore:
                dropChance = UnityEngine.Random.Range(0, 100);
                break;
            case MapEvent.Hunt:
                dropChance = UnityEngine.Random.Range(0, 50);
                break;
            case MapEvent.Slay:
                dropChance = UnityEngine.Random.Range(0, 20);
                break;
            case MapEvent.Raid:
                dropChance = UnityEngine.Random.Range(0, 15);
                break;
        }

        if (dropChance == 0)
        {
            int slot = UnityEngine.Random.Range(0, 8);
            switch (slot)
            {
                case 0:
                    Craft(username, "#freecraft! head");
                    break;
                case 1:
                    Craft(username, "#freecraft! chest");
                    break;
                case 2:
                    Craft(username, "#freecraft! mainhand");
                    break;
                case 3:
                    Craft(username, "#freecraft! offhand");
                    break;
                case 4:
                    Craft(username, "#freecraft! mantle");
                    break;
                case 5:
                    Craft(username, "#freecraft! belt");
                    break;
                case 6:
                    Craft(username, "#freecraft! ring");
                    break;
                case 7:
                    Craft(username, "#freecraft! amulet");
                    break;
            }
        }
    }

    private void Craft(string username, string message)
    {
        if ((message.ToLower().Split(' ')[0] == "!craft" || message.ToLower().Split(' ')[0] == "#freecraft!") && message.Split(' ').Length == 2)
        {
            Character c = GameData.character[GetCharacterIndex(username)];
            string slot = message.ToLower().Split(' ')[1];

            int tier = (c.level / 10) + 1;
            if (tier > 10)
            {
                tier = 10;
            }
            int cost = 1000 * tier;
            if (message.ToLower().Split(' ')[0] == "#freecraft!")
            {
                cost = 0;
            }

            int rng = UnityEngine.Random.Range(1, 100);
            string rarity = GetRarityFromRandom(rng);

            int currentItemValue = 0;
            int craftedItemValue = 0;

            if (c.gold >= cost)
            {
                c.gold -= cost;
                if (rarity == "Legendary")
                {
                    c.legendaryCount++;
                }
                string botMessage = String.Format("{0} crafted a {1}", username, rarity);
                if (message.ToLower().Split(' ')[0] == "#freecraft!")
                {
                    botMessage = String.Format("{0} looted a {1}", username, rarity);
                }
                switch (slot)
                {
                    case "mainhand":
                    case "mh":
                        currentItemValue = GetMainHandPowerValue(c);
                        craftedItemValue = (int)((float)tier * (5 + (tier - 1)) * GetRarityMultiplier(rarity));
                        if (message.ToLower().Split(' ')[0] == "#freecraft!")
                            botMessage += String.Format(" {0} tier {1}.", GetMainHandName(c.role), tier);
                        else
                            botMessage += String.Format(" {0} tier {1} for {2} gold and has {3} gold left.", GetMainHandName(c.role), tier, cost, c.gold);
                        if (craftedItemValue > currentItemValue)
                        {
                            c.mainHandSlot[0] = rarity;
                            c.mainHandSlot[1] = tier.ToString();
                            c.mainHandSlot[2] = "0";
                            botMessage += " It's now equiped!";
                        }
                        break;
                    case "offhand":
                    case "oh":
                        currentItemValue = GetOffHandPowerValue(c);
                        switch (c.role)
                        {
                            case "Knight":
                            case "Thief":
                            case "Archer":
                                craftedItemValue = (int)((float)tier * (32 + (tier - 1) * 2) * GetRarityMultiplier(rarity));
                                break;
                            case "Warrior":
                                craftedItemValue = (int)((float)tier * (3 + (tier - 1)) * GetRarityMultiplier(rarity));
                                break;
                            case "Priest":
                                craftedItemValue = (int)((float)tier * (10 + (tier - 1)) * GetRarityMultiplier(rarity));
                                break;
                        }
                        if (message.ToLower().Split(' ')[0] == "#freecraft!")
                            botMessage += String.Format(" {0} tier {1}.", GetOffHandName(c.role), tier);
                        else
                            botMessage += String.Format(" {0} tier {1} for {2} gold and has {3} gold left.", GetOffHandName(c.role), tier, cost, c.gold);
                        if (craftedItemValue > currentItemValue)
                        {
                            c.offHandSlot[0] = rarity;
                            c.offHandSlot[1] = tier.ToString();
                            c.offHandSlot[2] = "0";
                            botMessage += " It's now equiped!";
                        }
                        break;
                    case "head":
                        currentItemValue = GetHeadPowerValue(c);
                        craftedItemValue = (int)((float)tier * (48 + (tier - 1) * 2) * GetRarityMultiplier(rarity));
                        if (message.ToLower().Split(' ')[0] == "#freecraft!")
                            botMessage += String.Format(" {0} tier {1}.", GetHeadName(c.role), tier);
                        else
                            botMessage += String.Format(" {0} tier {1} for {2} gold and has {3} gold left.", GetHeadName(c.role), tier, cost, c.gold);
                        if (craftedItemValue > currentItemValue)
                        {
                            c.headSlot[0] = rarity;
                            c.headSlot[1] = tier.ToString();
                            c.headSlot[2] = "0";
                            botMessage += " It's now equiped!";
                        }
                        break;
                    case "chest":
                        currentItemValue = GetChestPowerValue(c);
                        craftedItemValue = (int)((float)tier * (48 + (tier - 1) * 2) * GetRarityMultiplier(rarity));
                        if (message.ToLower().Split(' ')[0] == "#freecraft!")
                            botMessage = String.Format(" {0} tier {1}.", GetChestName(c.role), tier);
                        else
                            botMessage += String.Format(" {0} tier {1} for {2} gold and has {3} gold left.", GetChestName(c.role), tier, cost, c.gold);
                        if (craftedItemValue > currentItemValue)
                        {
                            c.chestSlot[0] = rarity;
                            c.chestSlot[1] = tier.ToString();
                            c.chestSlot[2] = "0";
                            botMessage += " It's now equiped!";
                        }
                        break;
                    case "mantle":
                        currentItemValue = GetMantlePowerValue(c);
                        craftedItemValue = (int)((float)tier * (13 + (tier - 1) * 2) * GetRarityMultiplier(rarity));
                        if (message.ToLower().Split(' ')[0] == "#freecraft!")
                            botMessage = String.Format(" {0} tier Mantle.", tier);
                        else
                            botMessage += String.Format(" Mantle tier {0} for {1} gold and has {2} gold left.", tier, cost, c.gold);
                        if (craftedItemValue > currentItemValue || c.accessorySlot[3].ToLower() != slot)
                        {
                            c.accessorySlot[0] = rarity;
                            c.accessorySlot[1] = tier.ToString();
                            c.accessorySlot[2] = "0";
                            c.accessorySlot[3] = "Mantle";
                            botMessage += " It's now equiped!";
                        }
                        break;
                    case "amulet":
                        currentItemValue = GetAmuletPowerValue(c);
                        craftedItemValue = (int)((float)tier * (38 + (tier - 1) * 2) * GetRarityMultiplier(rarity));
                        if (message.ToLower().Split(' ')[0] == "#freecraft!")
                            botMessage += String.Format(" {0} tier Amulet.", tier);
                        else
                            botMessage += String.Format(" Amulet tier {0} for {1} gold and has {2} gold left.", tier, cost, c.gold);
                        if (craftedItemValue > currentItemValue || c.accessorySlot[3].ToLower() != slot)
                        {
                            c.accessorySlot[0] = rarity;
                            c.accessorySlot[1] = tier.ToString();
                            c.accessorySlot[2] = "0";
                            c.accessorySlot[3] = "Amulet";
                            botMessage += " It's now equiped!";
                        }
                        break;
                    case "ring":
                        currentItemValue = GetRingPowerValue(c);
                        craftedItemValue = (int)((float)tier * (13 + (tier - 1) * 2) * GetRarityMultiplier(rarity));
                        if (message.ToLower().Split(' ')[0] == "#freecraft!")
                            botMessage += String.Format(" {0} tier Mantle.", tier);
                        else
                            botMessage += String.Format(" Ring tier {0} for {1} gold and has {2} gold left.", tier, cost, c.gold);
                        if (craftedItemValue > currentItemValue || c.accessorySlot[3].ToLower() != slot)
                        {
                            c.accessorySlot[0] = rarity;
                            c.accessorySlot[1] = tier.ToString();
                            c.accessorySlot[2] = "0";
                            c.accessorySlot[3] = "Ring";
                            botMessage += " It's now equiped!";
                        }
                        break;
                    case "belt":
                        currentItemValue = GetBeltPowerValue(c);
                        craftedItemValue = (int)((float)tier * (1 + (tier - 1)) * GetRarityMultiplier(rarity));
                        if (message.ToLower().Split(' ')[0] == "#freecraft!")
                            botMessage += String.Format(" {0} tier Belt.", tier);
                        else
                            botMessage += String.Format(" Belt tier {0} for {1} gold and has {2} gold left.", tier, cost, c.gold);
                        if (craftedItemValue > currentItemValue || c.accessorySlot[3].ToLower() != slot)
                        {
                            c.accessorySlot[0] = rarity;
                            c.accessorySlot[1] = tier.ToString();
                            c.accessorySlot[2] = "0";
                            c.accessorySlot[3] = "Belt";
                            botMessage += " It's now equiped!";
                        }
                        break;
                    default:
                        SendTwitchMessage(String.Format("/w {0} WARNING: This equipment doesn't exist. !craft mainhand|offhand|head|chest|mantle|ring|amulet|belt.", c.username, cost, c.gold));
                        break;
                }

                if (botMessage != "")
                {
                    SendTwitchMessage(botMessage);
                    SendTwitchMessage("/w " + botMessage);
                }
            }
            else
            {
                SendTwitchMessage(String.Format("/w {0} WARNING: You don't have enough gold to craft ({2}/{1}).", c.username, cost, c.gold));
            }
        }
    }

    private void Join(string username, string message)
    {
        if (message.ToLower().Split(' ')[0] == "!join")
        {
            Character c = GameData.character[GetCharacterIndex(username)];
            if (c.antiSpamTimer <= 0)
            {
                c.antiSpamTimer = GameData.spamTimer;
                if (!IsCharacterJoined(username))
                {
                    if (!c.isTraining)
                    {
                        GameData.characterJoined.Add(c);
                        SendTwitchMessage(String.Format(" {0} has joined the next event as {1}.", c.username, c.role));
                    }
                    else
                    {
                        SendTwitchMessage(String.Format("/w {0} WARNING: You are currently training and cannot join the event.", c.username));
                    }
                }
            }
        }
    }

    private void Forge(string username, string message)
    {
        if (message.ToLower().Split(' ')[0] == "!forge")
        {
            Character c = GameData.character[GetCharacterIndex(username)];
            string slot = message.ToLower().Split(' ')[1];

            int forge = GetEquipmentForge(c, slot);
            int streamstoneCost = 0;

            if (forge >= 0 && forge < 5)
            {
                switch (forge)
                {
                    case 0:
                        streamstoneCost = 1;
                        break;
                    case 1:
                        streamstoneCost = 3;
                        break;
                    case 2:
                        streamstoneCost = 5;
                        break;
                    case 3:
                        streamstoneCost = 10;
                        break;
                    case 4:
                        streamstoneCost = 15;
                        break;

                }

                if (c.streamstone >= streamstoneCost)
                {
                    c.streamstone -= streamstoneCost;
                    int rng = UnityEngine.Random.Range(1, 100);

                    if (rng <= 40)
                    {
                        SetEquipmentForge(c, slot, 1);
                        SendTwitchMessage(String.Format("{0}'s {1} successfully forged to +{2}!", username, slot, forge + 1));
                    }
                    else if (rng >= 80)
                    {
                        if (forge > 0)
                        {
                            SetEquipmentForge(c, slot, -1);
                            SendTwitchMessage(String.Format("{0}'s {1} broke and lost 1 forge.", username, slot));
                        }
                        else
                        {
                            SendTwitchMessage(String.Format("{0}'s {1} failed to forge.", username, slot));
                        }
                    }
                    else
                    {
                        SendTwitchMessage(String.Format("{0}'s {1} failed to forge.", username, slot));
                    }
                }
                else
                {
                    SendTwitchMessage(String.Format("/w {0} WARNING: You don't have enough Streamstone ({2}/{1}).", c.username, streamstoneCost, c.streamstone));
                }
            }
            else
            {
                SendTwitchMessage(String.Format("/w {0} WARNING: This equipment is already maxed out at +5.", c.username));
            }
        }
    }

    private int GetEquipmentForge(Character c, string slot)
    {
        switch (slot)
        {
            case "mainhand":
                return Convert.ToInt32(c.mainHandSlot[2]);
            case "offhand":
                return Convert.ToInt32(c.offHandSlot[2]);
            case "head":
                return Convert.ToInt32(c.headSlot[2]);
            case "chest":
                return Convert.ToInt32(c.chestSlot[2]);
            case "mantle":
            case "amulet":
            case "ring":
            case "belt":
                return Convert.ToInt32(c.accessorySlot[2]);
            default:
                return -1;
        }
    }

    private void SetEquipmentForge(Character c, string slot, int next)
    {
        switch (slot)
        {
            case "mainhand":
                c.mainHandSlot[2] = (Convert.ToInt32(c.mainHandSlot[2]) + next).ToString();
                break;
            case "offhand":
                c.offHandSlot[2] = (Convert.ToInt32(c.offHandSlot[2]) + next).ToString();
                break;
            case "head":
                c.headSlot[2] = (Convert.ToInt32(c.headSlot[2]) + next).ToString();
                break;
            case "chest":
                c.chestSlot[2] = (Convert.ToInt32(c.chestSlot[2]) + next).ToString();
                break;
            case "mantle":
            case "amulet":
            case "ring":
            case "belt":
                c.accessorySlot[2] = (Convert.ToInt32(c.accessorySlot[2]) + next).ToString();
                break;
        }
    }


    private bool IsCharacterExist(string username)
    {
        foreach (Character c in GameData.character)
        {
            if (c.username == username)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsCharacterJoined(string username)
    {
        foreach (Character c in GameData.characterJoined)
        {
            if (c.username == username)
            {
                return true;
            }
        }
        return false;
    }

    private string FirstLetterToUpper(string str)
    {
        if (str == null)
            return null;

        if (str.Length > 1)
            return char.ToUpper(str[0]) + str.Substring(1);

        return str.ToUpper();
    }

    private float GetRarityMultiplier(string rarity)
    {
        float value = 0;

        switch (rarity)
        {
            case "Common":
                value = 1f;
                break;
            case "Uncommon":
                value = 1.25f;
                break;
            case "Rare":
                value = 1.5f;
                break;
            case "Epic":
                value = 2f;
                break;
            case "Legendary":
                value = 2.5f;
                break;
            default:
                break;
        }

        return value;
    }

    private string GetRarityFromRandom(int value)
    {
        string rarity = "";

        if (value <= 50)
        {
            rarity = "Common";
        }
        else if (value > 50 && value <= 75)
        {
            rarity = "Uncommon";
        }
        else if (value > 75 && value <= 90)
        {
            rarity = "Rare";
        }
        else if (value > 90 && value <= 97)
        {
            rarity = "Epic";
        }
        else
        {
            rarity = "Legendary";
        }

        return rarity;
    }

    private string GetMainHandName(string role)
    {
        string itemName = "";
        switch (role)
        {
            case "Knight":
                itemName = "Sword";
                break;
            case "Warrior":
                itemName = "Greatsword";
                break;
            case "Thief":
                itemName = "Dagger";
                break;
            case "Archer":
                itemName = "Bow";
                break;
            case "Wizard":
                itemName = "Staff";
                break;
            case "Priest":
                itemName = "Mace";
                break;
            default:
                break;
        }

        return itemName;
    }

    private string GetOffHandName(string role)
    {
        string itemName = "";
        switch (role)
        {
            case "Knight":
                itemName = "Shield";
                break;
            case "Warrior":
                itemName = "Gauntlet";
                break;
            case "Thief":
                itemName = "Dagger";
                break;
            case "Archer":
                itemName = "Arrow";
                break;
            case "Wizard":
                itemName = "Orb";
                break;
            case "Priest":
                itemName = "Book";
                break;
            default:
                break;
        }

        return itemName;
    }

    private string GetHeadName(string role)
    {
        string itemName = "";
        switch (role)
        {
            case "Knight":
                itemName = "Armet";
                break;
            case "Warrior":
                itemName = "Barbute";
                break;
            case "Thief":
                itemName = "FeatherCap";
                break;
            case "Archer":
                itemName = "Beret";
                break;
            case "Wizard":
                itemName = "MagicianHat";
                break;
            case "Priest":
                itemName = "Helmet";
                break;
            default:
                break;
        }

        return itemName;
    }

    private string GetChestName(string role)
    {
        string itemName = "";
        switch (role)
        {
            case "Knight":
                itemName = "FullPlate";
                break;
            case "Warrior":
                itemName = "Breastplate";
                break;
            case "Thief":
                itemName = "LeatherArmor";
                break;
            case "Archer":
                itemName = "LeatherArmor";
                break;
            case "Wizard":
                itemName = "Robe";
                break;
            case "Priest":
                itemName = "Robe";
                break;
            default:
                break;
        }

        return itemName;
    }

    public int GetMainHandPowerValue(Character c)
    {
        return (int)((float)(Convert.ToInt32(c.mainHandSlot[1]) + Convert.ToInt32(c.mainHandSlot[2])) * (5 + (Convert.ToInt32(c.mainHandSlot[1]) - 1)) * GetRarityMultiplier(c.mainHandSlot[0]));
    }

    public int GetOffHandPowerValue(Character c)
    {
        switch (c.role)
        {
            case "Knight":
            case "Thief":
            case "Archer":
            case "Wizard":
                return (int)((float)(Convert.ToInt32(c.offHandSlot[1]) + Convert.ToInt32(c.offHandSlot[2])) * (32 + ((Convert.ToInt32(c.offHandSlot[1]) - 1) * 2)) * GetRarityMultiplier(c.offHandSlot[0]));
            case "Warrior":
                return (int)((float)(Convert.ToInt32(c.offHandSlot[1]) + Convert.ToInt32(c.offHandSlot[2])) * (3 + (Convert.ToInt32(c.offHandSlot[1]) - 1)) * GetRarityMultiplier(c.offHandSlot[0]));
            case "Priest":
                return (int)((float)(Convert.ToInt32(c.offHandSlot[1]) + Convert.ToInt32(c.offHandSlot[2])) * (10 + (Convert.ToInt32(c.offHandSlot[1]) - 1)) * GetRarityMultiplier(c.offHandSlot[0]));
        }
        return 0;
    }

    public int GetHeadPowerValue(Character c)
    {
        return (int)((float)(Convert.ToInt32(c.headSlot[1]) + Convert.ToInt32(c.headSlot[2])) * (48 + (Convert.ToInt32(c.headSlot[1]) - 1) * 2) * GetRarityMultiplier(c.headSlot[0]));
    }

    public int GetChestPowerValue(Character c)
    {
        return (int)((float)(Convert.ToInt32(c.chestSlot[1]) + Convert.ToInt32(c.chestSlot[2])) * (48 + (Convert.ToInt32(c.chestSlot[1]) - 1) * 2) * GetRarityMultiplier(c.chestSlot[0]));
    }

    public int GetMantlePowerValue(Character c)
    {
        return (int)((float)(Convert.ToInt32(c.accessorySlot[1]) + Convert.ToInt32(c.accessorySlot[2])) * (13 + ((Convert.ToInt32(c.accessorySlot[1]) - 1) * 2)) * GetRarityMultiplier(c.accessorySlot[0]));
    }

    public int GetAmuletPowerValue(Character c)
    {
        return (int)((float)(Convert.ToInt32(c.accessorySlot[1]) + Convert.ToInt32(c.accessorySlot[2])) * (38 + (Convert.ToInt32(c.accessorySlot[1]) - 1) * 2) * GetRarityMultiplier(c.accessorySlot[0]));
    }

    public int GetRingPowerValue(Character c)
    {
        return (int)((float)(Convert.ToInt32(c.accessorySlot[1]) + Convert.ToInt32(c.accessorySlot[2])) * (13 + ((Convert.ToInt32(c.accessorySlot[1]) - 1) * 2)) * GetRarityMultiplier(c.accessorySlot[0]));
    }

    public int GetBeltPowerValue(Character c)
    {
        return (int)((float)(Convert.ToInt32(c.accessorySlot[1]) + Convert.ToInt32(c.accessorySlot[2])) * (1 + (Convert.ToInt32(c.accessorySlot[1]) - 1)) * GetRarityMultiplier(c.accessorySlot[0]));
    }

    public float GetPercentStats(int amount)
    {
        if (amount == 0)
        {
            return 0;
        }
        else
        {
            int min = 250;
            int max = 1875;
            double value = 0;

            value = (double)(amount + min) / (min + max) * 30;
            value = System.Math.Round(value, 2);
            return (float)value;
        }
    }

    private void GoldPerMinuteWatching()
    {
        goldWatchingTimer -= Time.deltaTime;

        if (goldWatchingTimer <= 0)
        {
            goldWatchingTimer = GameData.goldPerMinuteTimer;
            gameObject.GetComponent<LoadViewerList>().LoadViewer(channelName);

            foreach (Character c in GameData.character)
            {
                if (c.isOnline)
                {
                    int tier = (c.level / 10) + 1;
                    if (c.level == 100)
                    {
                        tier--;
                    }
                    c.gold += GameData.goldPerTierMultiplier * tier;
                    if (c.gold > GameData.maxGold)
                    {
                        c.gold = GameData.maxGold;
                    }
                }
            }

            Save();
        }
    }

    private void Save()
    {
        if (!File.Exists(Application.persistentDataPath + "/RPG_Stream.json"))
        {
            File.Create(Application.persistentDataPath + "/RPG_Stream.json");
        }
        else
        {
            string path = Application.persistentDataPath + "/RPG_Stream.json";
            File.WriteAllText(Application.persistentDataPath + "/RPG_Stream.json", "");
            
            StreamWriter writer = new StreamWriter(path, true);
            writer.WriteLine(saveJson.AddNewCharacterToJson());
            writer.Close();
        }
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    private void LaunchEvent()
    {
        if (GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y] == MapEvent.Explore)
        {
            GameObject.Find("Battle Info").GetComponent<CombatSystem>().InitCombat(MapEvent.Explore);
        }
        else if (GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y] == MapEvent.Hunt)
        {
            GameObject.Find("Battle Info").GetComponent<CombatSystem>().InitCombat(MapEvent.Hunt);
        }
        else if (GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y] == MapEvent.Slay)
        {
            GameObject.Find("Battle Info").GetComponent<CombatSystem>().InitCombat(MapEvent.Slay);
        }
        else if (GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y] == MapEvent.Raid)
        {
            GameObject.Find("Battle Info").GetComponent<CombatSystem>().InitCombat(MapEvent.Raid);
        }

        GameObject.Find("MiniMap").GetComponent<UI>().InitDpsMeter();
        GameData.showCombatFrame = true;
    }
}
