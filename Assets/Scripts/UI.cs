using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MapEvent
{
    None, City, Explore, Hunt, Slay, Raid, Success, Failure
}

public class UI : MonoBehaviour
{
    public Image[,] mapTiles = new Image[7, 7];
    public Sprite city;
    public Sprite explore;
    public Sprite hunt;
    public Sprite slay;
    public Sprite raid;
    public Text[] compass = new Text[4];
    public Text timer;
    public Text characterCount;
    public Text locationTitle;
    public Image bossHpBar;
    public Image knightHpBar;
    public Image warriorHpBar;
    public Image thiefHpBar;
    public Image archerHpBar;
    public Image wizardHpBar;
    public Image priestHpBar;
    private CombatSystem cs;
    public GameObject dpsBarPrefabs;
    public List<GameObject> dpsBar = new List<GameObject>();

    public GameObject ui;

    [HideInInspector]
    public Vector3 initPosition;

	void Start ()
    {
        for (int row = 0; row < 7; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                mapTiles[row, col] = GameObject.Find("x" + row + "_y" + col).GetComponent<Image>();
            }
        }

        cs = GameObject.Find("Battle Info").GetComponent<CombatSystem>();
        GameData.showCombatFrame = false;
        initPosition = transform.position;
        InitialMap();
	}

    void Update()
    {
        int knightsCurrentHP = 0;
        int knightsMaxHP = 0;
        int warriorsCurrentHP = 0;
        int warriorsMaxHP = 0;
        int thievesCurrentHP = 0;
        int thievesMaxHP = 0;
        int archersCurrentHP = 0;
        int archerMaxHP = 0;
        int wizardsCurrentHP = 0;
        int wizardsMaxHP = 0;
        int priestsCurrentHP = 0;
        int priestsMaxHP = 0;
        
        if (GameData.showMap)
        {
            transform.position = initPosition;
        }
        else
        {
            transform.position = new Vector2(-5000, -5000);
        }

        if (Input.GetMouseButton(1))
        {
            transform.position = Input.mousePosition;
            initPosition = Input.mousePosition;

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                float alpha = transform.GetChild(0).GetComponent<Image>().color.a;
                if (alpha < 0)
                    alpha = 0;
                if (alpha > 1)
                    alpha = 1;

                transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, alpha + scroll * 0.1f);
            }
        }

        ShowCompass();

        if (GameData.mapTimer / 120 < 1)
        {
            if ((GameData.mapTimer % 60) % 1 >= 0.5f)
            {
                timer.color = Color.red;
            }
            else
            {
                timer.color = Color.white;
            }
        }
        else
        {
            timer.color = Color.white;
        }

        timer.text = (int)GameData.mapTimer / 60 + "m" + (int)GameData.mapTimer % 60 + "s";

        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 6; col++)
            {
                if (GameData.map[row, col] == MapEvent.Success)
                {
                    HighlightTile(row, col, Color.green);
                }

                if (GameData.map[row, col] == MapEvent.Failure)
                {
                    HighlightTile(row, col, Color.red);
                }
            }
        }

        HighlightTile(3, 3, Color.blue);
        HighlightTile((int)GameData.mapPosition.x, (int)GameData.mapPosition.y, Color.yellow);

        if (GameData.characterJoined.Count <= 1)
        {
            characterCount.text = "!join";
        }
        else
        {
            characterCount.text = GameData.characterJoined.Count.ToString();
        }

        switch (GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y].ToString())
        {
            case "Explore":
            case "Hunt":
            case "Slay":
            case "Raid":
            case "City":
                locationTitle.text = /*"(" + (int)GameData.mapPosition.x + ", " + (int)GameData.mapPosition.y + ") " + */GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y].ToString();
                break;
        }

        //Combat Frame
        if (cs.isFighting)
        {
            List<Character> knights = KnightAlive();
            List<Character> warriors = WarriorAlive();
            List<Character> thieves = ThiefAlive();
            List<Character> archers = ArcherAlive();
            List<Character> wizards = WizardAlive();
            List<Character> priests = PriestAlive();

            foreach (Character c in knights)
            {
                knightsCurrentHP += c.stats.currentHP;
                knightsMaxHP += c.stats.maxHP;
            }

            foreach (Character c in warriors)
            {
                warriorsCurrentHP += c.stats.currentHP;
                warriorsMaxHP += c.stats.maxHP;
            }

            foreach (Character c in thieves)
            {
                thievesCurrentHP += c.stats.currentHP;
                thievesMaxHP += c.stats.maxHP;
            }

            foreach (Character c in archers)
            {
                archersCurrentHP += c.stats.currentHP;
                archerMaxHP += c.stats.maxHP;
            }

            foreach (Character c in wizards)
            {
                wizardsCurrentHP += c.stats.currentHP;
                wizardsMaxHP += c.stats.maxHP;
            }

            foreach (Character c in priests)
            {
                priestsCurrentHP += c.stats.currentHP;
                priestsMaxHP += c.stats.maxHP;
            }

            knightHpBar.transform.parent.GetComponentInChildren<Text>().text = "Knights (x" + KnightAlive().Count + ")";
            warriorHpBar.transform.parent.GetComponentInChildren<Text>().text = "Warriors (x" + WarriorAlive().Count + ")";
            thiefHpBar.transform.parent.GetComponentInChildren<Text>().text = "Thieves (x" + ThiefAlive().Count + ")";
            archerHpBar.transform.parent.GetComponentInChildren<Text>().text = "Archers (x" + ArcherAlive().Count + ")";
            wizardHpBar.transform.parent.GetComponentInChildren<Text>().text = "Wizards (x" + WizardAlive().Count + ")";
            priestHpBar.transform.parent.GetComponentInChildren<Text>().text = "Priests (x" + PriestAlive().Count + ")";

            bossHpBar.fillAmount = (float)cs.boss.currentHP / (float)cs.boss.maxHP;
            knightHpBar.fillAmount = (float)knightsCurrentHP / (float)knightsMaxHP;
            warriorHpBar.fillAmount = (float)warriorsCurrentHP / (float)warriorsMaxHP;
            thiefHpBar.fillAmount = (float)thievesCurrentHP / (float)thievesMaxHP;
            archerHpBar.fillAmount = (float)archersCurrentHP / (float)archerMaxHP;
            wizardHpBar.fillAmount = (float)wizardsCurrentHP / (float)wizardsMaxHP;
            priestHpBar.fillAmount = (float)priestsCurrentHP / (float)priestsMaxHP;

            ui.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 360);

            if (GameData.showDpsMeter)
            {
                List<Character> topDPS = SortByDamageDealt();

                int barNumber = 0;
                int highestDPS = topDPS[0].damageDealt;
                foreach (Character c in topDPS)
                {
                    if (barNumber < 5)
                    {
                        dpsBar[barNumber].transform.GetChild(0).GetComponent<Text>().text = c.username;
                        dpsBar[barNumber].transform.GetChild(1).GetComponent<Text>().text = c.damageDealt.ToString();

                        if (highestDPS > 0)
                        {
                            dpsBar[barNumber].GetComponent<Image>().fillAmount = (float)c.damageDealt / (float)highestDPS;
                        }
                        else
                        {
                            dpsBar[barNumber].GetComponent<Image>().fillAmount = 1;
                        }

                        switch (c.role)
                        {
                            case "Knight":
                                dpsBar[barNumber].GetComponent<Image>().color = new Color(0, 0.5f, 1);
                                break;
                            case "Warrior":
                                dpsBar[barNumber].GetComponent<Image>().color = new Color(1, 0.5f, 0);
                                break;
                            case "Thief":
                                dpsBar[barNumber].GetComponent<Image>().color = new Color(1, 0.75f, 0);
                                break;
                            case "Wizard":
                                dpsBar[barNumber].GetComponent<Image>().color = new Color(0.5f, 0, 1);
                                break;
                            case "Archer":
                                dpsBar[barNumber].GetComponent<Image>().color = new Color(0.5f, 1, 0);
                                break;
                            case "Priest":
                                dpsBar[barNumber].GetComponent<Image>().color = new Color(1, 0, 1);
                                break;
                        }
                        barNumber++;
                    }
                    else
                        break;
                }

                GameObject.Find("DPS Meter").GetComponent<RectTransform>().anchoredPosition = new Vector2(521, 220);
            }
        }
        else
        {
            GameObject.Find("DPS Meter").GetComponent<RectTransform>().anchoredPosition = new Vector2(-5000, -5000);
            ui.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 99999);
        }
    }

    private void ShowCompass()
    {
        int resetCount = 0;
        for (int i = 0; i < 4; i++)
        {
            compass[i].GetComponent<Text>().text = "";
        }

        if (GameData.voteNorth != null)
        {
            if ((int)GameData.mapPosition.y < 6)
            {
                if (GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y + 1] != MapEvent.None &&
                    GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y + 1] != MapEvent.Success &&
                    GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y + 1] != MapEvent.Failure)
                {
                    compass[0].GetComponent<Text>().text = "N (" + GameData.voteNorth.Count.ToString() + ")";
                    GameData.canVote[0] = true;
                }
                else
                {
                    GameData.canVote[0] = false;
                    resetCount++;
                }
            }
            else
            {
                GameData.canVote[0] = false;
                resetCount++;
            }
        }

        if (GameData.voteEast != null)
        {
            if (GameData.mapPosition.x < 6)
            {
                if (GameData.map[(int)GameData.mapPosition.x + 1, (int)GameData.mapPosition.y] != MapEvent.None &&
                    GameData.map[(int)GameData.mapPosition.x + 1, (int)GameData.mapPosition.y] != MapEvent.Success &&
                    GameData.map[(int)GameData.mapPosition.x + 1, (int)GameData.mapPosition.y] != MapEvent.Failure)
                {
                    compass[1].GetComponent<Text>().text = "E (" + GameData.voteEast.Count.ToString() + ")";
                    GameData.canVote[1] = true;
                }
                else
                {
                    GameData.canVote[1] = false;
                    resetCount++;
                }
            }
            else
            {
                GameData.canVote[1] = false;
                resetCount++;
            }
        }

        if (GameData.voteSouth != null)
        {
            if (GameData.mapPosition.y > 0)
            {
                if (GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y - 1] != MapEvent.None &&
                    GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y - 1] != MapEvent.Success &&
                    GameData.map[(int)GameData.mapPosition.x, (int)GameData.mapPosition.y - 1] != MapEvent.Failure)
                {
                    compass[2].GetComponent<Text>().text = "S (" + GameData.voteSouth.Count.ToString() + ")";
                    GameData.canVote[2] = true;
                }
                else
                {
                    GameData.canVote[2] = false;
                    resetCount++;
                }
            }
            else
            {
                GameData.canVote[2] = false;
                resetCount++;
            }
        }

        if (GameData.voteWest != null)
        {
            if (GameData.mapPosition.x > 0)
            {
                if (GameData.map[(int)GameData.mapPosition.x - 1, (int)GameData.mapPosition.y] != MapEvent.None &&
                    GameData.map[(int)GameData.mapPosition.x - 1, (int)GameData.mapPosition.y] != MapEvent.Success &&
                    GameData.map[(int)GameData.mapPosition.x - 1, (int)GameData.mapPosition.y] != MapEvent.Failure)
                {
                    compass[3].GetComponent<Text>().text = "W (" + GameData.voteWest.Count.ToString() + ")";
                    GameData.canVote[3] = true;
                }
                else
                {
                    GameData.canVote[3] = false;
                    resetCount++;
                }
            }
            else
            {
                GameData.canVote[3] = false;
                resetCount++;
            }
        }

        if (resetCount == 4)
        {
            GameObject.Find("MiniMap").GetComponent<UI>().InitialMap();
            GameObject.Find(">MainObject<").GetComponent<ChatCommand>().SendTwitchMessage(String.Format("You are now in The City (3, 3). !craft and !forge are now available."));
        }
    }

    public void InitialMap()
    {
        for (int row = 0; row < 7; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                mapTiles[row, col].color = new Color(0, 0, 0, 0);
                mapTiles[row, col].transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(0, 0, 0, 0);
                GameData.map[row, col] = MapEvent.None;
            }
        }

        SelectRandomPattern();
    }

    private void SetTileSprite(int x, int y, Sprite sprite)
    {
        mapTiles[x, y].sprite = sprite;
        mapTiles[x, y].color = new Color(1, 1, 1, 1);

        switch(sprite.name.ToLower())
        {
            case "city":
                GameData.map[x, y] = MapEvent.City;
                break;
            case "explore":
                GameData.map[x, y] = MapEvent.Explore;
                break;
            case "hunt":
                GameData.map[x, y] = MapEvent.Hunt;
                break;
            case "slay":
                GameData.map[x, y] = MapEvent.Slay;
                break;
            case "raid":
                GameData.map[x, y] = MapEvent.Raid;
                break;
            default:
                GameData.map[x, y] = MapEvent.None;
                break;
        }
    }

    private void HighlightTile(int x, int y, Color color)
    {
        mapTiles[x, y].transform.GetChild(0).GetComponentInChildren<Image>().color = color;
    }

    private List<Character> KnightAlive()
    {
        List<Character> knights = new List<Character>();

        foreach (Character c in GameData.characterJoined)
        {
            if (c.role == "Knight" && c.stats.currentHP > 0)
            {
                knights.Add(c);
            }
        }
        return knights;
    }

    private List<Character> WarriorAlive()
    {
        List<Character> warriors = new List<Character>();

        foreach (Character c in GameData.characterJoined)
        {
            if (c.role == "Warrior" && c.stats.currentHP > 0)
            {
                warriors.Add(c);
            }
        }
        return warriors;
    }

    private List<Character> ThiefAlive()
    {
        List<Character> thieves = new List<Character>();

        foreach (Character c in GameData.characterJoined)
        {
            if (c.role == "Thief" && c.stats.currentHP > 0)
            {
                thieves.Add(c);
            }
        }
        return thieves;
    }

    private List<Character> ArcherAlive()
    {
        List<Character> archers = new List<Character>();

        foreach (Character c in GameData.characterJoined)
        {
            if (c.role == "Archer" && c.stats.currentHP > 0)
            {
                archers.Add(c);
            }
        }
        return archers;
    }

    private List<Character> WizardAlive()
    {
        List<Character> wizards = new List<Character>();

        foreach (Character c in GameData.characterJoined)
        {
            if (c.role == "Wizard" && c.stats.currentHP > 0)
            {
                wizards.Add(c);
            }
        }
        return wizards;
    }

    private List<Character> PriestAlive()
    {
        List<Character> priests = new List<Character>();

        foreach (Character c in GameData.characterJoined)
        {
            if (c.role == "Priest" && c.stats.currentHP > 0)
            {
                priests.Add(c);
            }
        }
        return priests;
    }


    private List<Character> SortByDamageDealt()
    {
        int[] array = new int[GameData.characterJoined.Count];

        for (int i = 0; i < GameData.characterJoined.Count; i++)
        {
            array[i] = GameData.characterJoined[i].damageDealt;
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
                if (GameData.characterJoined[k].damageDealt == array[i])
                {
                    bool isAlreadyInArray = false;
                    for (int m = 0; m < arrayName.Count; m++)
                    {
                        if (arrayName[m].username == GameData.characterJoined[k].username)
                        {
                            isAlreadyInArray = true;
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

    public void InitDpsMeter()
    {
        GameData.showDpsMeter = true;
        foreach (GameObject bars in dpsBar)
        {
            Destroy(bars);
        }
        dpsBar.Clear();

        int spawnDpsBar = 0;
        foreach (Character c in GameData.characterJoined)
        {
            if (spawnDpsBar < 5)
            {
                GameObject bar = Instantiate(dpsBarPrefabs, Vector2.zero, Quaternion.identity);
                dpsBar.Add(bar);
                bar.transform.parent = GameObject.Find("DPS Meter").gameObject.transform;
                c.damageDealt = 0;
                spawnDpsBar++;
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// [0,6] [1,6] [2,6] [3,6] [4,6] [5,6] [6,6]
    /// [0,5] [1,5] [2,5] [3,5] [4,5] [5,5] [6,5]
    /// [0,4] [1,4] [2,4] [3,4] [4,4] [5,4] [6,4]
    /// [0,3] [1,3] [2,3] [3|3] [4,3] [5,3] [6,3]
    /// [0,2] [1,2] [2,2] [3,2] [4,2] [5,2] [6,2]
    /// [0,1] [1,1] [2,1] [3,1] [4,1] [5,1] [6,1]
    /// [0,0] [1,0] [2,0] [3,0] [4,0] [5,0] [6,0]
    /// </summary>
    /// <param name="tiles"></param>

    private void SelectRandomPattern()
    {
        int rng = UnityEngine.Random.Range(1, 4);

        switch (rng)
        {
            case 1:
                Pattern1();
                break;
            case 2:
                Pattern2();
                break;
            case 3:
                Pattern3();
                break;
        }
        GameData.mapPosition.x = 3;
        GameData.mapPosition.y = 3;
    }

    private void Pattern1()
    {
        SetTileSprite(3, 3, city);
        SetTileSprite(4, 0, hunt);
        SetTileSprite(5, 0, hunt);
        SetTileSprite(2, 1, hunt);
        SetTileSprite(5, 1, hunt);
        SetTileSprite(1, 2, slay);
        SetTileSprite(2, 2, explore);
        SetTileSprite(3, 2, explore);
        SetTileSprite(5, 2, explore);
        SetTileSprite(0, 3, raid);
        SetTileSprite(4, 3, explore);
        SetTileSprite(5, 3, explore);
        SetTileSprite(6, 3, hunt);
        SetTileSprite(0, 4, slay);
        SetTileSprite(3, 4, explore);
        SetTileSprite(0, 5, hunt);
        SetTileSprite(2, 5, explore);
        SetTileSprite(3, 5, explore);
        SetTileSprite(4, 5, hunt);
        SetTileSprite(5, 5, explore);
        SetTileSprite(6, 5, explore);
        SetTileSprite(0, 6, hunt);
        SetTileSprite(1, 6, explore);
        SetTileSprite(2, 6, hunt);
        SetTileSprite(6, 6, slay);
    }

    private void Pattern2()
    {
        SetTileSprite(3, 3, city);
        SetTileSprite(1, 5, slay);
        SetTileSprite(2, 5, hunt);
        SetTileSprite(3, 5, explore);
        SetTileSprite(4, 5, slay);
        SetTileSprite(5, 5, slay);
        SetTileSprite(6, 5, slay);
        SetTileSprite(3, 4, explore);
        SetTileSprite(6, 4, raid);
        SetTileSprite(1, 3, explore);
        SetTileSprite(2, 3, explore);
        SetTileSprite(4, 3, hunt);
        SetTileSprite(1, 2, hunt);
        SetTileSprite(4, 2, explore);
        SetTileSprite(5, 2, hunt);
        SetTileSprite(0, 1, slay);
        SetTileSprite(1, 1, explore);
        SetTileSprite(3, 1, hunt);
        SetTileSprite(4, 1, explore);
        SetTileSprite(1, 0, slay);
    }

    private void Pattern3()
    {
        SetTileSprite(3, 3, city);
        SetTileSprite(2, 5, slay);
        SetTileSprite(4, 5, slay);
        SetTileSprite(1, 4, raid);
        SetTileSprite(2, 4, hunt);
        SetTileSprite(4, 4, hunt);
        SetTileSprite(5, 4, explore);
        SetTileSprite(2, 3, explore);
        SetTileSprite(4, 3, explore);
        SetTileSprite(1, 2, slay);
        SetTileSprite(2, 2, explore);
        SetTileSprite(4, 2, explore);
        SetTileSprite(5, 2, slay);
        SetTileSprite(2, 1, hunt);
        SetTileSprite(4, 1, hunt);
    }

}
