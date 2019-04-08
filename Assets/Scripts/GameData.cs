using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class GameData
{
    static public List<Character> character = new List<Character>();
    static public List<string> viewer = new List<string>();
    static public List<Character> characterJoined = new List<Character>();
    static public MapEvent[,] map = new MapEvent[7,7];
    static public List<string> voteNorth = new List<string>();
    static public List<string> voteEast = new List<string>();
    static public List<string> voteSouth = new List<string>();
    static public List<string> voteWest = new List<string>();
    static public Vector2 mapPosition = new Vector2(3, 3);

    static public float spamTimer = 1f;
    static public float trainingTick = 600f;
    static public float goldPerMinuteTimer = 60f;
    static public int goldPerTierMultiplier = 2;
    static public int trainingExperience = 3;
    static public int maxGold = 50000;
    static public int maxStreamstone = 20;
    static public float pvpDuelTimer = 30f;
    static public bool showMap = false;
    static public bool showCombatFrame = false;
    static public float mapTimer = 12f;
    static public bool isCombatDebug = true;
    static public bool showDpsMeter = true;
}
