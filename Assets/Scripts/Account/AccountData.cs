using System;
using System.Collections.Generic;

[System.Serializable]
public class AccountData 
{
    public string playerName;
    public string playerID;
    public int localPlayerID;
    public string lastSaveTime;
    public int avatarCode;
    public int avatarBackgroundCode;
    public int nameBackgroundCode;
    public List<int> cosmeticCodes;
    public int gems;
    public int experience;
    public int level;
    public AccountStatus accountStatus;
    public string premiumEndDate;


}



