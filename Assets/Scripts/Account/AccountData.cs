using System;
using System.Collections.Generic;

[System.Serializable]
public class AccountData 
{


    public string playerName;
    public string playerID;
    //public int localPlayerID;
    //public string lastSaveTime;
    public int avatarCode;
    public int avatarBackgroundCode;
    public int nameBackgroundCode;
    public List<int> cosmeticCodes;
    public int gems;
    public int experience;
    public int level;
    public AccountStatus accountStatus;
    public string premiumEndDate;

    public bool completeTutorial;
    public AccountData()
    {
        
    }
    public AccountData(AccountData other)
    {
        playerName = other.playerName;
        playerID = other.playerID;
        avatarCode = other.avatarCode;
        avatarBackgroundCode = other.avatarBackgroundCode;
        nameBackgroundCode = other.nameBackgroundCode;
        cosmeticCodes = new List<int>(other.cosmeticCodes);
        gems = other.gems;
        experience = other.experience;
        level = other.level;
        accountStatus = other.accountStatus;
        premiumEndDate = other.premiumEndDate;
        completeTutorial = other.completeTutorial;
        // המהאי ³םר³ ןמכÿ, ÿךשמ ÷
    }

}



