using System;
using System.Collections.Generic;

[System.Serializable]
public class AccountData 
{


    public string playerName;
    public string playerID;    
    public int avatarCode;
    public int avatarBackgroundCode;
    public int nameBackgroundCode;
    public List<int> cosmeticCodes;
    public int gems;
    public int experience;
    public int level;
    public AccountStatus accountStatus;
    

    public bool hasLifetimeSubscription;
    public DateTime subscriptionEndDate;

    public bool specialOfferEarned;
    public TutorialStatus tutorialStatus;
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
        subscriptionEndDate = other.subscriptionEndDate;
        tutorialStatus = other.tutorialStatus;
        specialOfferEarned = other.specialOfferEarned;
        hasLifetimeSubscription = other.hasLifetimeSubscription;
    }

}



