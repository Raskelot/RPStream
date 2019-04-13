using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatSystem : MonoBehaviour
{
    [HideInInspector]
    public bool isFighting = false;
    public Boss boss = new Boss();
    
    private float combatTimer = 2f;
    private int combatTurn = 0;
    private ChatCommand cc;

    void Start ()
    {
        cc = GameObject.Find(">MainObject<").GetComponent<ChatCommand>();
    }
	
	void Update ()
    {
        LaunchCombat();
	}

    private void LaunchCombat()
    {
        if (isFighting)
        {
            //isCombatStart = false;
            combatTimer -= Time.deltaTime;

            if (combatTimer <= 0)
            {
                combatTimer = 1f;
                combatTurn++;

                if (!IsDefeated())
                {
                    if (!IsVictorious())
                    {
                        ChatCommand chatCommand = GameObject.Find(">MainObject<").GetComponent<ChatCommand>();
                        List<Character> characterAlive = SortByCharacterAlive();
                        List<Character> tanks = GetTanksAlive();
                        //Character turn
                        foreach (Character c in GameData.characterJoined)
                        {
                            if (c.stats.currentHP > 0)
                            {
                                //Critical Chance
                                int critChance = UnityEngine.Random.Range(0, 10000);
                                int damageDealt = 0;
                                if (critChance <= (int)(chatCommand.GetPercentStats(c.stats.critical) * 100))
                                {
                                    damageDealt = (int)(c.stats.power * 1.5);
                                    if (GameData.isCombatDebug)
                                    {
                                        cc.SendTwitchMessage(String.Format("/w {0} You critical strike for {1} at turn {2}!", c.username, damageDealt.ToString(), combatTurn));
                                    }
                                }
                                else
                                {
                                    damageDealt = c.stats.power;
                                }
                                boss.currentHP -= damageDealt;
                                c.damageDealt += damageDealt;

                                //Healing every 3 turns
                                if (combatTurn % 3 == 1)
                                {
                                    if (c.role == "Priest")
                                    {
                                        bool tankHealed = false;

                                        foreach (Character tank in tanks)
                                        {
                                            if (tank.stats.currentHP / tank.stats.maxHP < 0.8f)
                                            {
                                                tankHealed = true;

                                                if (critChance <= (int)(chatCommand.GetPercentStats(c.stats.critical) * 100))
                                                {
                                                    tank.stats.currentHP += (int)(c.stats.healPower * 1.5);
                                                }
                                                else
                                                {
                                                    tank.stats.currentHP += c.stats.healPower;
                                                }

                                                if (tank.stats.currentHP > tank.stats.maxHP)
                                                {
                                                    tank.stats.currentHP = tank.stats.maxHP;
                                                }
                                                break;
                                            }
                                        }

                                        if (!tankHealed)
                                        {
                                            foreach (Character other in characterAlive)
                                            {
                                                if (other.stats.currentHP < other.stats.maxHP)
                                                {
                                                    if (critChance <= (int)(chatCommand.GetPercentStats(c.stats.critical) * 100))
                                                    {
                                                        other.stats.currentHP += (int)(c.stats.healPower * 1.5);
                                                    }
                                                    else
                                                    {
                                                        other.stats.currentHP += c.stats.healPower;
                                                    }

                                                    if (other.stats.currentHP > other.stats.maxHP)
                                                    {
                                                        other.stats.currentHP = other.stats.maxHP;
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        GameObject.Find("BossFrame").GetComponentInChildren<Text>().text = boss.currentHP.ToString();

                        //Boss attack non-tanks every 3 turns
                        if (combatTurn % 3 == 0)
                        {
                            foreach (Character ch in characterAlive)
                            {
                                ch.stats.currentHP -= boss.power;
                            }
                        }

                        //At least one tank alive
                        if (tanks.Count > 0)
                        {
                            int selectRandomTank = UnityEngine.Random.Range(0, tanks.Count);

                            int evadeRNG = UnityEngine.Random.Range(1, 10000);
                            if (evadeRNG <= tanks[selectRandomTank].stats.evasion)
                            {
                                cc.SendTwitchMessage(String.Format("/w {0} You evaded at turn {1}!", tanks[selectRandomTank].username, combatTurn));
                            }
                            else
                            {
                                tanks[selectRandomTank].stats.currentHP -= boss.power;
                            }
                        }
                        else
                        {
                            int selectRandomParty = UnityEngine.Random.Range(0, characterAlive.Count);
                            GameData.characterJoined[selectRandomParty].stats.currentHP -= boss.power;
                        }
                            
                    }
                }
            }

        }
    }

    public void InitCombat(MapEvent type)
    {
        boss.type = type;

        ChatCommand chatCommand = GameObject.Find(">MainObject<").GetComponent<ChatCommand>();
        foreach (Character c in GameData.characterJoined)
        {
            c.stats.maxHP = (c.level * 20) + chatCommand.GetHeadPowerValue(c) + chatCommand.GetChestPowerValue(c);
            c.stats.power = (c.level * 5) + chatCommand.GetMainHandPowerValue(c);
            c.stats.evasion = 0;
            c.stats.critical = 0;
            c.stats.healPower = 0;
            c.stats.absorb = 0;

            //OffHand effect
            switch (c.role)
            {
                case "Knight":
                case "Wizard":
                    c.stats.evasion += chatCommand.GetOffHandPowerValue(c);
                    break;
                case "Warrior":
                    c.stats.power += chatCommand.GetOffHandPowerValue(c);
                    break;
                case "Thief":
                case "Archer":
                    c.stats.critical += chatCommand.GetOffHandPowerValue(c);
                    break;
                case "Priest":
                    c.stats.healPower += chatCommand.GetOffHandPowerValue(c);
                    break;
                default:
                    break;
            }

            //Accessory effect
            switch (c.accessorySlot[3])
            {
                case "Mantle":
                    c.stats.evasion += chatCommand.GetMantlePowerValue(c);
                    break;
                case "Amulet":
                    c.stats.maxHP += chatCommand.GetAmuletPowerValue(c);
                    break;
                case "Ring":
                    c.stats.critical += chatCommand.GetRingPowerValue(c);
                    break;
                case "Belt":
                    c.stats.power += chatCommand.GetBeltPowerValue(c);
                    break;
                default:
                    break;
            }

            c.stats.currentHP = c.stats.maxHP;
        }

        boss.tier = (GetAverageLevel() / 10) + 1;
        switch (boss.type)
        {
            case MapEvent.Explore:
                boss.maxHP = ((150 * boss.tier) + (20 * GetAverageLevel())) * GameData.characterJoined.Count;
                boss.power = boss.tier + ((3 * GetAverageLevel()) * GameData.characterJoined.Count);
                break;
            case MapEvent.Hunt:
                boss.maxHP = ((150 * boss.tier) + (40 * GetAverageLevel())) * GameData.characterJoined.Count;
                boss.power = boss.tier + ((4 * GetAverageLevel()) * GameData.characterJoined.Count);
                break;
            case MapEvent.Slay:
                boss.maxHP = ((150 * boss.tier) + (60 * GetAverageLevel())) * GameData.characterJoined.Count;
                boss.power = boss.tier + ((5 * GetAverageLevel()) * GameData.characterJoined.Count);
                break;
            case MapEvent.Raid:
                boss.maxHP = ((175 * boss.tier) + (80 * GetAverageLevel())) * GameData.characterJoined.Count;
                boss.power = boss.tier + ((6 * GetAverageLevel()) * GameData.characterJoined.Count);
                break;

        }
        boss.currentHP = boss.maxHP;
        
        switch (boss.type)
        {
            case MapEvent.Explore:
                boss.experience = 10;
                boss.gold = 20 * boss.tier;
                break;
            case MapEvent.Hunt:
                boss.experience = 12;
                boss.gold = 40 * boss.tier;
                if (UnityEngine.Random.Range(1, 100) <= 10)
                {
                    boss.streamstone = 1;
                }
                break;
            case MapEvent.Slay:
                boss.experience = 15;
                boss.gold = 60 * boss.tier;
                if (UnityEngine.Random.Range(1, 100) <= 25)
                {
                    boss.streamstone = 1;
                }
                break;
            case MapEvent.Raid:
                boss.gold = 2000;
                boss.streamstone = UnityEngine.Random.Range(1, 4);
                break;
        }

        isFighting = true;
    }
        
    private bool IsDefeated()
    {
        int partyFeinted = 0;
        foreach (Character c in GameData.characterJoined)
        {
            if (c.stats.currentHP <= 0)
            {
                partyFeinted++;
            }
        }

        if (partyFeinted == GameData.characterJoined.Count)
        {
            foreach (Character c in GameData.characterJoined)
            {
                c.pveDefeat++;
            }
            isFighting = false;
            foreach (Character c in GameData.characterJoined)
            {
                cc.SendTwitchMessage(String.Format("/w {3} Defeat in {0} (X:{1}, Y:{2})! You dealt {3} ({4})% damage.", 
                    GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y], (int)GameData.mapPosition.x, (int)GameData.mapPosition.y, c.username, c.damageDealt, (c.damageDealt / boss.maxHP) * 100));
            }
            GameData.showCombatFrame = false;
            GameData.showDpsMeter = false;
            GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y] = MapEvent.Failure;
            GameData.characterJoined.Clear();
            cc.SendTwitchMessage(String.Format("Defeat!"));
            return true;
        }

        return false;
    }

    private bool IsVictorious()
    {
        if (boss.currentHP <= 0)
        {
            foreach (Character c in GameData.characterJoined)
            {
                c.experience += boss.experience * (GetAverageLevel() / c.level);
                c.gold += boss.gold;
                c.goldReceived += boss.gold;
                c.streamstone += boss.streamstone;
                c.streamstoneReceived += boss.streamstone;
                c.pveVictory++;
                cc.SendTwitchMessage(String.Format("/w {3} Victory! You earn {0} gold, {1}% experience and {2} streamstone. You dealt {3} ({4})% damage.", 
                    boss.gold, boss.experience * (GetAverageLevel() / c.level), boss.streamstone, c.username, c.damageDealt, (int)((float)(c.damageDealt / boss.maxHP) * 100)));
                cc.DropGear(c.username);
            }
            isFighting = false;
            GameData.showCombatFrame = false;
            GameData.showDpsMeter = false;
            GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y] = MapEvent.Success;
            GameData.characterJoined.Clear();
            cc.SendTwitchMessage(String.Format("Victory!"));
            return true;
        }
        return false;
    }

    private List<Character> GetTanksAlive()
    {
        List<Character> tanks = new List<Character>();
        foreach (Character c in GameData.characterJoined)
        {
            if (c.role == "Knight" && c.stats.currentHP > 0)
            {
                tanks.Add(c);
            }
        }
        return tanks;
    }

    private List<Character> SortByCharacterAlive()
    {
        int[] array = new int[GameData.characterJoined.Count];

        for (int i = 0; i < GameData.characterJoined.Count; i++)
        {
            array[i] = GameData.characterJoined[i].stats.currentHP;
        }
        Array.Sort(array);

        int[] copy = new int[GameData.characterJoined.Count];
        Array.Copy(array, copy, GameData.characterJoined.Count);
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = copy[(copy.Length - 1) - i];
        }

        List<Character> arrayName = new List<Character>();

        for (int i = 0; i < array.Length; i++)
        {
            for (int k = 0; k < GameData.characterJoined.Count; k++)
            {
                if (GameData.characterJoined[k].stats.currentHP == array[i])
                {
                    bool isAlreadyInArray = false;
                    for (int m = 0; m < arrayName.Count; m++)
                    {
                        if (arrayName[m].username == GameData.characterJoined[k].username)
                        {
                            if (arrayName[m].stats.currentHP > 0)
                            {
                                isAlreadyInArray = true;
                            }
                        }
                    }
                    if (!isAlreadyInArray)
                    {
                        arrayName.Add(GameData.characterJoined[k]);
                    }
                }
            }
        }
        
        return arrayName;
    }

    public int GetAverageLevel()
    {
        int totalLevel = 0;
        int totalPlayers = 0;

        foreach (Character c in GameData.characterJoined)
        {
            totalLevel += c.level;
            totalPlayers++;
        }
        return (totalLevel / totalPlayers);
    }
}
