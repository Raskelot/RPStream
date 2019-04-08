/// <summary>
/// Not working for this project
/// </summary>

[System.Serializable]
public class SaveState
{
    public string username { get; set; }
    public string role { get; set; }
    public int prestige { get; set; }

    public int level { get; set; }

    public int gold { get; set; }
    public int streamstone { get; set; }
    public int experience { get; set; }

    public int totalTrainingMinute { get; set; }
    public int pveVictory { get; set; }
    public int pveDefeat { get; set; }
    public int pvpVictory { get; set; }
    public int pvpDefeat { get; set; }
    public int legendaryCount { get; set; }
    public int goldReceived { get; set; }
    public int streamstoneReceived { get; set; }

    //Equipments Rarity|Tier|Forge|AccessoryType
    public string[] mainHandSlot = new string[3];
    public string[] offHandSlot = new string[3];
    public string[] headSlot = new string[3];
    public string[] chestSlot = new string[3];
    public string[] accessorySlot = new string[4];
}
